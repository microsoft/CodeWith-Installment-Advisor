import React, { useState } from 'react';
import {
    Button,
    Input,
    makeStyles,
    tokens,
} from '@fluentui/react-components';
import { Send24Regular } from '@fluentui/react-icons';

const useStyles = makeStyles({
    inputContainer: {
        padding: tokens.spacingVerticalM,
        backgroundColor: tokens.colorNeutralBackground1,
    },
    input: {
        width: '100%',
    },
    sendButton: {
        minWidth: 'auto',
        padding: '4px',
    },
});

interface ChatInputProps {
    loading: boolean;
    onSendMessage: (message: string) => void;
}

export const ChatInput: React.FC<ChatInputProps> = ({ loading, onSendMessage }) => {
    const [input, setInput] = useState('');
    const styles = useStyles();

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        if (!input.trim() || loading) return;

        onSendMessage(input);
        setInput('');
    };

    const handleKeyPress = (e: React.KeyboardEvent) => {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            handleSubmit(e);
        }
    };

    return (
        <form className={styles.inputContainer} onSubmit={handleSubmit}>
            <Input
                className={styles.input}
                value={input}
                appearance='filled-darker'
                size="large"
                onChange={(_, data) => setInput(data.value)}
                onKeyDown={handleKeyPress}
                placeholder="Typ je bericht..."
                disabled={loading}
                contentAfter={
                    <Button
                        appearance="transparent"
                        icon={<Send24Regular />}
                        className={styles.sendButton}
                        onClick={handleSubmit}
                        disabled={loading || !input.trim()}
                        aria-label={loading ? 'Versturen...' : 'Verstuur bericht'}
                        size="small"
                    />
                }
            />
        </form>
    );
};
