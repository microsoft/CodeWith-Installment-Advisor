import React, { useEffect, useRef } from 'react';
import {
    makeStyles,
    tokens,
} from '@fluentui/react-components';
import { ChatMessage } from './ChatMessage';
import { ChatMessage as ChatMessageType } from '../types';

const useStyles = makeStyles({
    container: {
        flex: 1,
        overflowY: 'auto',
        padding: tokens.spacingVerticalM,
        display: 'flex',
        flexDirection: 'column',
        gap: tokens.spacingVerticalS,
    },
});

interface ChatMessagesProps {
    messages: ChatMessageType[];
}

export const ChatMessages: React.FC<ChatMessagesProps> = ({ messages }) => {
    const styles = useStyles();
    const chatEndRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        if (chatEndRef.current) {
            const container = chatEndRef.current.parentElement;
            if (container) {
                const isScrolledToBottom =
                    container.scrollHeight - container.clientHeight <= container.scrollTop + 1;

                // Only auto-scroll if user is already at or near the bottom
                if (isScrolledToBottom || messages.length <= 2) {
                    chatEndRef.current.scrollIntoView({ behavior: 'smooth' });
                }
            }
        }
    }, [messages]);

    return (
        <div className={styles.container}>
            {messages.map((message, index) => (
                <ChatMessage key={index} message={message} />
            ))}
            <div ref={chatEndRef} />
        </div>
    );
};
