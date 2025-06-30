import React from 'react';
import {
    Button,
    Input,
    makeStyles,
    Popover,
    PopoverSurface,
    PopoverTrigger,
    tokens
} from '@fluentui/react-components';
import {
    Person24Regular,
    WeatherMoon24Regular,
    WeatherSunny24Regular,
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

interface TopBarProps {
    darkMode: boolean;
    onToggleDarkMode: () => void;
    onUserIdChange: (userId: string) => void;
    userId: string;
}

export const TopBar: React.FC<TopBarProps> = ({
    darkMode,
    onToggleDarkMode,
    onUserIdChange,
    userId,
}) => {
    const styles = useStyles();

    return (
        <div className={styles.topbar}>
            <div className={styles.text}>Installment Advisor Sample App</div>
            <div className={styles.actions}>
                <Button
                    appearance="subtle"
                    icon={darkMode ? <WeatherMoon24Regular /> : <WeatherSunny24Regular />}
                    onClick={onToggleDarkMode}
                    className={styles.text}
                    title={darkMode ? 'Light mode' : 'Dark mode'}
                />
                <Popover withArrow>
                    <PopoverTrigger disableButtonEnhancement>
                        <Button
                            appearance="subtle"
                            icon={<Person24Regular />}
                            className={styles.text}
                        >
                            {userId}
                        </Button>
                    </PopoverTrigger>
                    <PopoverSurface>
                        <div className={styles.userMenu}>
                            <Input
                                value={userId}
                                onChange={(_, data) => onUserIdChange(data.value)}
                                autoFocus
                            />
                        </div>
                    </PopoverSurface>
                </Popover>
            </div>
        </div>
    );
};
