import React from 'react';
import {
    Button,
    makeStyles,
    tokens,
} from '@fluentui/react-components';
import { ScenarioCard as ScenarioCardType } from '../types';

const useStyles = makeStyles({
    container: {
        display: 'flex',
        gap: tokens.spacingHorizontalL,
        margin: `${tokens.spacingVerticalL} ${tokens.spacingHorizontalM}`,
        flexWrap: 'wrap',
    },
    card: {
        height: 'auto',
        flex: 1,
        padding: tokens.spacingVerticalL,
        textAlign: 'left',
        whiteSpace: 'pre-line',
        border: `1px solid ${tokens.colorBrandBackground2}`,
        borderRadius: tokens.borderRadiusMedium,
        transition: 'all 0.2s ease',
        ':hover': {
            border: `2px solid ${tokens.colorBrandBackground2Hover}`,
            transform: 'translateY(-1px)',
            boxShadow: tokens.shadow8,
        },
        ':active': {
            transform: 'translateY(0)',
        },
    },
});

interface ScenarioCardsProps {
    scenarios: ScenarioCardType[];
    onScenarioClick: (scenarioText: string) => void;
    loading: boolean;
}

export const ScenarioCards: React.FC<ScenarioCardsProps> = ({
    scenarios,
    onScenarioClick,
    loading
}) => {
    const styles = useStyles();

    return (
        <div className={styles.container}>
            {scenarios.map((scenario, index) => (
                <Button
                    key={index}
                    className={styles.card}
                    appearance="subtle"
                    onClick={() => onScenarioClick(scenario.text)}
                    disabled={loading}
                >
                    {scenario.text}
                </Button>
            ))}
        </div>
    );
};
