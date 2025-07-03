import { useCallback } from 'react';
import { ChatMessage } from '../types';

interface StreamingState {
    streamedText: string;
    isReceivingImage: boolean;
    imageBuffer: string;
    pendingImages: string[];
    hasReceivedFirstContent: boolean;
    currentTool: string;
    responseStarted: boolean;
    hasReceivedFirstToolCall: boolean;
}

interface UseStreamingOptions {
    setMessages: React.Dispatch<React.SetStateAction<ChatMessage[]>>;
    setThreadId: (id: string | null) => void;
}

export const useStreaming = ({ setMessages, setThreadId }: UseStreamingOptions) => {

    const processStreamingResponse = useCallback(async (response: Response) => {
        if (!response.body) {
            throw new Error('No response body for streaming');
        }

        const reader = response.body.getReader();
        const decoder = new TextDecoder();

        // Initialize streaming state
        const state: StreamingState = {
            streamedText: '',
            isReceivingImage: false,
            imageBuffer: '',
            pendingImages: [],
            hasReceivedFirstContent: false,
            currentTool: '',
            responseStarted: false,
            hasReceivedFirstToolCall: false,
        };

        // Process the stream
        while (true) {
            const { value, done } = await reader.read();
            if (done) break;

            const chunk = decoder.decode(value, { stream: true });

            // Handle different chunk types, returns true if it was a metadata chunk.
            if (await handleChunk(chunk, state)) {
                continue;
            }
        }
        // Final cleanup
        await handleStreamEnd(state);

    }, [setMessages, setThreadId]);

    const handleChunk = useCallback(async (chunk: string, state: StreamingState): Promise<boolean> => {

        // Always started with a thread ID.
        if (chunk.includes('[STARTED THREAD]:')) {
            const parts = chunk.split(':');
            if (parts.length > 1) {
                const newThreadId = parts[1].trim();
                setThreadId(newThreadId);
                console.log('Set new threadId:', newThreadId);
            }
            return true;
        }

        // Handle tool calls
        if (!state.responseStarted && chunk.includes('[TOOLCALL]')) {
            handleToolCalls(chunk, state);
            return true;
        }

        // Handle response start
        if (!state.responseStarted && chunk.includes('[STARTED RESPONSE]:')) {
            handleResponseStart(chunk, state);
            return true;
        }

        // Handle images.
        if (chunk.includes('[IMAGE]:') || state.isReceivingImage) {
            handleImageChunk(chunk, state);
            return true;
        }

        // Handle content chunks only after response has started
        if (state.responseStarted) {
            handleContentChunk(chunk, state);
        }

        return false;
    }, [setMessages, setThreadId]);

    const handleToolCalls = useCallback((chunk: string, state: StreamingState) => {
        const toolCallRegex = /\[TOOLCALL\]\s*([^[]+?)\s*\[ENDTOOLCALL\]/g;
        let match;

        while ((match = toolCallRegex.exec(chunk)) !== null) {
            const toolName = match[1].trim();
            if (toolName) {
                state.currentTool = toolName;
                var typingInfo = state.currentTool;

                if (toolName === 'visualization-agent') {
                    typingInfo = 'Using the Visualization Agent to generate a nice looking chart, this can take a few seconds...';
                } else if (toolName === 'scenario-agent') {
                    typingInfo = 'Using the Scenario Agent to analyze the current scenario, please hold on...';
                } else if (toolName === 'installment-rule-evaluation-agent') {
                    typingInfo = 'Using the Installment Rule Evaluation Agent to evaluate the possibilities, please give me a few seconds...';
                }

                // Update typing indicator message with tool calls
                if (!state.hasReceivedFirstToolCall) {
                    state.hasReceivedFirstToolCall = true;
                    setMessages(prev => [
                        ...prev.slice(0, -1), // Remove original typing indicator
                        {
                            sender: 'bot',
                            text: '',
                            images: [],
                            typingInfo: typingInfo,
                            isTyping: true // Keep as typing message but with tool calls
                        }
                    ]);
                } else {
                    setMessages(prev => {
                        const updated = [...prev];
                        updated[updated.length - 1] = {
                            sender: 'bot',
                            text: '',
                            images: [],
                            typingInfo: typingInfo,
                            isTyping: true // Keep as typing message
                        };
                        return updated;
                    });

                }
            }
        }
    }, [setMessages]);

    const handleResponseStart = useCallback((chunk: string, state: StreamingState) => {
        state.responseStarted = true;

        // Extract the content after [STARTED RESPONSE]: 
        const responseRegex = /\[STARTED RESPONSE\]:\s*(.*)/;
        const responseMatch = chunk.match(responseRegex);
        if (responseMatch && responseMatch[1]) {
            const firstContentChunk = responseMatch[1];
            console.log('First content chunk from STARTED RESPONSE:', firstContentChunk);

            // Process this first chunk as regular content
            if (firstContentChunk.trim()) {
                // Remove typing indicator on first content
                if (!state.hasReceivedFirstContent) {
                    state.hasReceivedFirstContent = true;
                    setMessages(prev => [
                        ...prev.slice(0, -1), // Remove typing indicator
                        {
                            sender: 'bot',
                            text: '',
                            images: [],
                            typingInfo: state.currentTool,
                            isTyping: false
                        }
                    ]);
                }

                state.streamedText += firstContentChunk;

                // Update the message with the first content
                updateBotMessage(state);
            }
        }
    }, [setMessages]);

    const handleContentChunk = useCallback((chunk: string, state: StreamingState) => {
        // Handle image chunks
        if (chunk.includes('[IMAGE]:') || state.isReceivingImage) {
            handleImageChunk(chunk, state);
        } else {
            // Regular text chunk
            handleTextChunk(chunk, state);
        }
    }, []);

    const handleImageChunk = useCallback((chunk: string, state: StreamingState) => {
        // Remove typing indicator on first content
        if (!state.hasReceivedFirstContent) {
            state.hasReceivedFirstContent = true;
            setMessages(prev => [
                ...prev.slice(0, -1), // Remove typing indicator
                {
                    sender: 'bot',
                    text: '',
                    images: [],
                    typingInfo: state.currentTool,
                    isTyping: false
                }
            ]);
        }

        // Start of a new image
        if (chunk.includes('[IMAGE]:') && !state.isReceivingImage) {
            const imageStartIndex = chunk.indexOf('[IMAGE]:');
            const textBeforeImage = chunk.substring(0, imageStartIndex);
            const imageDataStart = chunk.substring(imageStartIndex + '[IMAGE]:'.length);

            // Add any text before the image marker
            if (textBeforeImage) {
                state.streamedText += textBeforeImage;
            }

            // Start collecting image data
            state.isReceivingImage = true;
            state.imageBuffer = imageDataStart;

        } else if (state.isReceivingImage) {
            // All chunks are part of the image until [IMAGE_END]
            state.imageBuffer += chunk;
        }

        // Check if image is complete with [IMAGE_END] marker
        if (state.isReceivingImage && chunk.includes('[IMAGE_END]')) {
            const endIndex = state.imageBuffer.indexOf('[IMAGE_END]');

            if (endIndex !== -1) {
                // Extract just the image data (before [IMAGE_END])
                const finalImageData = state.imageBuffer.substring(0, endIndex);

                if (finalImageData.trim()) {
                    state.pendingImages.push(finalImageData.trim());
                }

                // Extract any text after [IMAGE_END]
                const remainingText = state.imageBuffer.substring(endIndex + '[IMAGE_END]'.length);
                if (remainingText) {
                    state.streamedText += remainingText;
                }

                state.imageBuffer = '';
                state.isReceivingImage = false;
            }
        }

        // Update message with current text and images
        updateBotMessage(state);
    }, [setMessages]);

    const handleTextChunk = useCallback((chunk: string, state: StreamingState) => {
        // Remove typing indicator on first content
        if (!state.hasReceivedFirstContent) {
            state.hasReceivedFirstContent = true;
            setMessages(prev => [
                ...prev.slice(0, -1), // Remove typing indicator
                {
                    sender: 'bot',
                    text: '',
                    images: [],
                    typingInfo: state.currentTool,
                    isTyping: false
                }
            ]);
        }

        state.streamedText += chunk;

        updateBotMessage(state);
    }, [setMessages]);


    const updateBotMessage = useCallback((state: StreamingState) => {
        setMessages(prev => {
            const updated = [...prev];
            updated[updated.length - 1] = {
                sender: 'bot',
                text: state.streamedText,
                images: [...state.pendingImages],
                typingInfo: state.currentTool,
                isTyping: false
            };
            return updated;
        });
    }, [setMessages]);

    const handleStreamEnd = useCallback(async (state: StreamingState) => {
        // Handle any remaining image data when stream ends
        if (state.isReceivingImage && state.imageBuffer.trim()) {
            state.pendingImages.push(state.imageBuffer.trim());
        }

        // Final update with all collected data
        if (state.pendingImages.length > 0 || state.streamedText.trim()) {
            updateBotMessage(state);
        } else {
            // If no content was streamed, show error
            setMessages(prev => [
                ...prev.slice(0, -1), // Remove empty bot message
                {
                    sender: 'bot',
                    text: 'Er is een fout opgetreden. Probeer het later opnieuw.',
                    images: [],
                    typingInfo: state.currentTool,
                    isTyping: false
                }
            ]);
        }
    }, [setMessages, updateBotMessage]);

    return {
        processStreamingResponse
    };
};
