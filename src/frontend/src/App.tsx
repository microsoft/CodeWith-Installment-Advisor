import { makeStyles, tokens } from '@fluentui/react-components';
import { TopBar, ChatMessages, ChatInput, ScenarioCards } from './components';
import { useChat } from './hooks/useChat';
import { ChatHeader } from 'components/ChatHeader';
import { InfoSection } from 'components/InfoSection';

const useStyles = makeStyles({
    container: {
        display: 'flex',
        flexDirection: 'column',
        height: '100vh',
        width: '100vw',
        backgroundColor: tokens.colorNeutralBackground1,
    },
    main: {
        display: 'flex',
        flexDirection: 'column',
        flex: 1,
        overflow: 'hidden',
        '@media (min-width: 1024px)': {
            flexDirection: 'row',
        },
    },
    chatSection: {
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        backgroundColor: tokens.colorNeutralBackground2,
        flex: 5,
        overflow: 'hidden',
        maxWidth: '100%',
        '@media (min-width: 1024px)': {
            maxWidth: 'calc(100% - 350px)',
        },
    },
    chatContainer: {
        display: 'flex',
        margin: 'auto',
        flexDirection: 'column',
        flex: 1,
        backgroundColor: tokens.colorNeutralBackground1,
        borderRadius: tokens.borderRadiusXLarge,
        boxShadow: tokens.shadow4,
        overflow: 'hidden',
        height: '80%',
        width: '100%',
        maxWidth: '800px'
    }
});

interface AppProps {
    darkMode: boolean;
    toggleDarkMode: () => void;
}

export function App({ darkMode, toggleDarkMode }: AppProps) {
    const styles = useStyles();
    const {
        messages,
        loading,
        userId,
        streaming,
        scenarioCards,
        hasUserChatted,
        sendMessage,
        startNewChat,
        resetUserId,
        toggleStreaming,
    } = useChat(); return (
        <div className={styles.container}>
            <TopBar
                darkMode={darkMode}
                userId={userId}
                onToggleDarkMode={toggleDarkMode}
                onUserIdChange={resetUserId}
                streaming={streaming}
                onToggleStreaming={toggleStreaming}
            />
            <div className={styles.main}>
                <InfoSection/>
                <div className={styles.chatSection}>
                    <div className={styles.chatContainer}>
                        <ChatHeader
                            onNewChat={startNewChat}
                        />
                        <ChatMessages messages={messages} streaming={streaming} />
                        {!hasUserChatted && (
                            <ScenarioCards
                                scenarios={scenarioCards}
                                onScenarioClick={sendMessage}
                                loading={loading}
                            />
                        )}
                        <ChatInput loading={loading} onSendMessage={sendMessage} />
                    </div>
                </div>
            </div>
        </div>
    );
}