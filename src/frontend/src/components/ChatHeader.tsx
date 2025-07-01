import React from 'react';
import {
    Button,
    makeStyles,
    tokens,
} from '@fluentui/react-components';
import {
    Add24Regular,
} from '@fluentui/react-icons';

const useStyles = makeStyles({
    topbar: {
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
        padding: `${tokens.spacingVerticalM} ${tokens.spacingHorizontalL}`,
        backgroundColor: tokens.colorBrandBackground,
        height: '40px',
    },
    text: {
        fontSize: tokens.fontSizeBase400,
        fontWeight: tokens.fontWeightSemibold,
        color: tokens.colorNeutralForegroundOnBrand,
    },
    actions: {
        display: 'flex',
        alignItems: 'center',
        gap: tokens.spacingHorizontalS,
    },
    userMenu: {
        padding: tokens.spacingVerticalM,
        minWidth: '200px',
    },
    userMenuLabel: {
        marginBottom: tokens.spacingVerticalXS,
    },
});

interface ChatHeaderProps {
    onNewChat: () => void;
}

export const ChatHeader: React.FC<ChatHeaderProps> = ({
    onNewChat,
}) => {
    const styles = useStyles();

    return (
        <div className={styles.topbar}>
            <div className={styles.text}>Chat</div>
            <div className={styles.actions}>
                <Button
                    appearance="subtle"
                    icon={<Add24Regular />}
                    onClick={onNewChat}
                    className={styles.text}
                    title="Start new chat"
                />      
            </div>
        </div>
    );
};
