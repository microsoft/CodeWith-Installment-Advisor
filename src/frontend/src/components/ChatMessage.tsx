import React from 'react';
import {
    makeStyles,
    tokens,
    ProgressBar,
} from '@fluentui/react-components';
import ReactMarkdown from 'react-markdown';
import { ChatMessage as ChatMessageType } from '../types';

const useStyles = makeStyles({
    message: {
        marginBottom: tokens.spacingVerticalM,
        display: 'flex',
        flexDirection: 'column',
    },
    userMessage: {
        alignSelf: 'flex-end',
        maxWidth: '70%',
    },
    botMessage: {
        alignSelf: 'flex-start',
        maxWidth: '70%',
    },
    messageContent: {
        padding: tokens.spacingVerticalM,
        borderRadius: tokens.borderRadiusMedium,
        wordWrap: 'break-word',
    },
    userContent: {
        backgroundColor: tokens.colorBrandBackground,
        color: tokens.colorNeutralForegroundOnBrand,
    },
    botContent: {
        backgroundColor: tokens.colorNeutralBackground3,
        color: tokens.colorNeutralForeground1,
    },
    typingIndicator: {
        display: 'flex',
        flexDirection: 'column',
        gap: tokens.spacingVerticalXS,
        padding: tokens.spacingVerticalM,
        backgroundColor: tokens.colorNeutralBackground3,
        borderRadius: tokens.borderRadiusMedium,
        minWidth: '120px',
    },
    toolCalls: {
        display: 'flex',
        flexDirection: 'column',
        gap: tokens.spacingVerticalXXS,
        marginBottom: tokens.spacingVerticalXS,
    },
    toolCall: {
        fontSize: tokens.fontSizeBase200,
        color: tokens.colorNeutralForeground2,
        fontStyle: 'italic',
    },
    progressContainer: {
        display: 'flex',
        alignItems: 'center',
        gap: tokens.spacingHorizontalS,
    },
    images: {
        display: 'flex',
        flexDirection: 'column',
        gap: tokens.spacingVerticalS,
        marginTop: tokens.spacingVerticalS,
    },
    image: {
        width: '100%',
        height: 'auto',
        borderRadius: tokens.borderRadiusSmall,
        objectFit: 'cover',
        maxHeight: '400px', // Prevent extremely tall images
    },
    markdown: {
        '& p': {
            margin: 0,
            marginBottom: tokens.spacingVerticalXS,
        },
        '& p:last-child': {
            marginBottom: 0,
        },
        '& ul, & ol': {
            marginTop: tokens.spacingVerticalXS,
            marginBottom: tokens.spacingVerticalXS,
            paddingLeft: tokens.spacingHorizontalL,
        },
        '& li': {
            marginBottom: tokens.spacingVerticalXXS,
        },
        '& code': {
            backgroundColor: tokens.colorNeutralBackground4,
            padding: `${tokens.spacingVerticalXXS} ${tokens.spacingHorizontalXS}`,
            borderRadius: tokens.borderRadiusSmall,
            fontFamily: tokens.fontFamilyMonospace,
        },
        '& pre': {
            backgroundColor: tokens.colorNeutralBackground4,
            padding: tokens.spacingVerticalS,
            borderRadius: tokens.borderRadiusSmall,
            overflow: 'auto',
            '& code': {
                backgroundColor: 'transparent',
                padding: 0,
            },
        },
    },
});

interface ChatMessageProps {
    message: ChatMessageType;
    streaming: boolean;
}

export const ChatMessage: React.FC<ChatMessageProps> = ({ message, streaming }) => {
    const styles = useStyles();

    // Loader including tool call info in case of streaming.
    if (message.isTyping) {
        return (
            <div className={`${styles.message} ${styles.botMessage}`}>
                <div className={styles.typingIndicator}>
                    {(streaming && message.typingInfo?.trim() != '') && (
                        <div className={styles.toolCalls}>
                            <div className={styles.toolCall}>
                                {message.typingInfo}
                            </div>
                        </div>
                    )}
                    <div className={styles.progressContainer}>
                        <ProgressBar />
                    </div>
                </div>
            </div>
        );
    }

    return (
        <div className={`${styles.message} ${message.sender === 'user' ? styles.userMessage : styles.botMessage}`}>
            <div className={`${styles.messageContent} ${message.sender === 'user' ? styles.userContent : styles.botContent}`}>
                {message.sender === 'bot' ? (
                    <div className={styles.markdown}>
                        <ReactMarkdown>{message.text}</ReactMarkdown>
                    </div>
                ) : (
                    message.text
                )}
                {message.images && message.images.length > 0 && (
                    <div className={styles.images}>
                        {message.images.map((image, index) => (
                            <img
                                key={index}
                                src={`data:image/png;base64,${image}`}
                                alt={`Response ${index + 1}`}
                                className={styles.image}
                                onError={(e) => {
                                    console.error('Failed to load image:', e);
                                    (e.target as HTMLImageElement).style.display = 'none';
                                }}
                            />
                        ))}
                    </div>
                )}
            </div>
        </div>
    );
};
