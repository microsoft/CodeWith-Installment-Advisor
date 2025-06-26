import React, { useState, useRef, useEffect } from 'react';
import ReactMarkdown from 'react-markdown';
import './App.css';

const API_ENDPOINT = (process.env.REACT_APP_CHAT_API || 'https://localhost:55018').replace(/\/+$/, '') + '/chat';

function App() {
  const [messages, setMessages] = useState([
    { sender: 'bot', text: 'Welkom bij de Installment Advisor! Stel hier je vraag over je termijnbedrag.' }
  ]);
  const [input, setInput] = useState('');
  const [loading, setLoading] = useState(false);
  const [threadId, setThreadId] = useState(null);
  const [darkMode, setDarkMode] = useState(false);
  const [userId, setUserId] = useState('23456');
  const [userMenuOpen, setUserMenuOpen] = useState(false);
  const chatEndRef = useRef(null);

  useEffect(() => {
    if (chatEndRef.current) {
      const messagesContainer = chatEndRef.current.parentElement;
      const isScrolledToBottom = messagesContainer.scrollHeight - messagesContainer.clientHeight <= messagesContainer.scrollTop + 1;
      
      // Only auto-scroll if user is already at or near the bottom
      if (isScrolledToBottom || messages.length <= 2) {
        chatEndRef.current.scrollIntoView({ behavior: 'smooth' });
      }
    }
  }, [messages]);

  useEffect(() => {
    document.body.classList.toggle('dark-mode', darkMode);
  }, [darkMode]);

  const sendMessage = async (e) => {
    e.preventDefault();
    if (!input.trim()) return;
    const userMessage = { sender: 'user', text: input };
    const currentInput = input;
    setMessages((msgs) => [...msgs, userMessage]);
    setInput(''); // Clear input immediately
    setLoading(true);
    
    // Add typing indicator
    const typingMessage = { sender: 'bot', text: '', isTyping: true };
    setMessages((msgs) => [...msgs, typingMessage]);
    
    try {
      const body = {
        UserID: userId,
        message: currentInput,
        ...(threadId ? { threadId } : {})
      };
      const res = await fetch(API_ENDPOINT, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(body)
      });
      const data = await res.json();
      if (data.threadId) setThreadId(data.threadId);
      
      // Remove typing indicator and add actual response
      setMessages((msgs) => [
        ...msgs.slice(0, -1), // Remove typing indicator
        { 
          sender: 'bot', 
          text: data.reply || data.message || 'Geen antwoord ontvangen.',
          images: data.images || []
        }
      ]);
    } catch (err) {
      // Remove typing indicator and add error message
      setMessages((msgs) => [
        ...msgs.slice(0, -1), // Remove typing indicator
        { sender: 'bot', text: 'Er is een fout opgetreden. Probeer het later opnieuw.', images: [] }
      ]);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="chat-fullscreen">
      <div className="chat-topbar">
        <div className="chat-title">Installment Advisor Chat</div>
        <div className="chat-topbar-actions">
          <button
            className="toggle-mode-btn"
            aria-label={darkMode ? 'Switch to light mode' : 'Switch to dark mode'}
            onClick={() => setDarkMode((d) => !d)}
            title={darkMode ? 'Licht modus' : 'Donkere modus'}
          >
            {darkMode ? '🌙' : '☀️'}
          </button>
          <div className="user-menu-wrapper">
            <button
              className="user-login-btn"
              onClick={() => setUserMenuOpen((open) => !open)}
              aria-expanded={userMenuOpen}
              aria-haspopup="true"
              type="button"
            >
              <span className="user-avatar">👤</span> {userId}
            </button>
            {userMenuOpen && (
              <div className="user-menu-dropdown">
                <label htmlFor="user-id-input" className="user-menu-label">Gebruiker ID:</label>
                <input
                  id="user-id-input"
                  className="user-id-input"
                  type="text"
                  value={userId}
                  onChange={e => setUserId(e.target.value)}
                  autoFocus
                />
              </div>
            )}
          </div>
        </div>
      </div>
      <div className="chat-main">
        <div className="chat-messages">
          {messages.map((msg, idx) => (
            <div key={idx} className={`chat-message ${msg.sender}`}>
              {msg.sender === 'bot' ? (
                <div>
                  {msg.isTyping ? (
                    <div className="typing-indicator">
                      <div className="typing-dots">
                        <span></span>
                        <span></span>
                        <span></span>
                      </div>
                      <span className="typing-text"></span>
                    </div>
                  ) : (
                    <>
                      <ReactMarkdown>{msg.text}</ReactMarkdown>
                      {msg.images && msg.images.length > 0 && (
                        <div className="message-images">
                          {msg.images.map((image, imageIdx) => (
                            <img
                              key={imageIdx}
                              src={`data:image/png;base64,${image}`}
                              alt={`Response image ${imageIdx + 1}`}
                              className="response-image"
                              onError={(e) => {
                                console.error('Failed to load image:', e);
                                e.target.style.display = 'none';
                              }}
                            />
                          ))}
                        </div>
                      )}
                    </>
                  )}
                </div>
              ) : (
                msg.text
              )}
            </div>
          ))}
          <div ref={chatEndRef} />
        </div>
        <form className="chat-input-row" onSubmit={sendMessage}>
          <input
            type="text"
            value={input}
            onChange={e => setInput(e.target.value)}
            placeholder="Typ je bericht..."
            disabled={loading}
          />
          <button type="submit" disabled={loading || !input.trim()}>
            {loading ? '...' : 'Verstuur'}
          </button>
        </form>
      </div>
    </div>
  );
}

export default App;
