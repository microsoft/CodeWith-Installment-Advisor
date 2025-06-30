import { useState, useCallback } from 'react';
import { ChatMessage, ScenarioCard } from '../types';
import { ChatApiService } from '../services/ChatApiService';

const INITIAL_MESSAGE: ChatMessage = {
  sender: 'bot',
  text: 'Welkom bij de Installment Advisor! Stel hier je vraag over je termijnbedrag.'
};

const SCENARIO_CARDS: ScenarioCard[] = [
  {
    text: 'Ik wil mijn termijnbedrag graag zo aanpassen dat ik aan het eind niet hoef bij te betalen.'
  },
  {
    text: 'Welke energieprijzen betaal ik op dit moment'
  },
  {
    text: 'Ik kreeg vorig jaar geld terug, maar toch stijgt mijn termijnbedrag. Waarom?'
  }
];

export const useChat = () => {
  const [messages, setMessages] = useState<ChatMessage[]>([INITIAL_MESSAGE]);
  const [loading, setLoading] = useState(false);
  const [threadId, setThreadId] = useState<string | null>(null);
  const [userId, setUserId] = useState('23456');

  const sendMessage = useCallback(async (messageText: string) => {
    if (!messageText.trim() || loading) return;

    const userMessage: ChatMessage = { sender: 'user', text: messageText };
    setMessages(prev => [...prev, userMessage]);
    setLoading(true);

    // Add typing indicator
    const typingMessage: ChatMessage = { sender: 'bot', text: '', isTyping: true };
    setMessages(prev => [...prev, typingMessage]);

    try {
      const response = await ChatApiService.sendMessage({
        UserID: userId,
        message: messageText,
        ...(threadId ? { threadId } : {})
      });

      if (response.threadId) {
        setThreadId(response.threadId);
      }

      // Remove typing indicator and add actual response
      setMessages(prev => [
        ...prev.slice(0, -1), // Remove typing indicator
        {
          sender: 'bot',
          text: response.reply || response.message || 'Geen antwoord ontvangen.',
          images: response.images || []
        }
      ]);
    } catch (error) {
      console.error('Chat error:', error);
      // Remove typing indicator and add error message
      setMessages(prev => [
        ...prev.slice(0, -1), // Remove typing indicator
        {
          sender: 'bot',
          text: 'Er is een fout opgetreden. Probeer het later opnieuw.',
          images: []
        }
      ]);
    } finally {
      setLoading(false);
    }
  }, [loading, threadId, userId]);

  const startNewChat = useCallback(() => {
    setMessages([INITIAL_MESSAGE]);
    setThreadId(null);
    setLoading(false);
  }, []);

  const resetUserId = (userId: string) => {
    setUserId(userId);
    startNewChat();
  };

  const hasUserChatted = messages.some(m => m.sender === 'user');

  return {
    messages,
    loading,
    userId,
    scenarioCards: SCENARIO_CARDS,
    hasUserChatted,
    sendMessage,
    startNewChat,
    resetUserId,
  };
};
