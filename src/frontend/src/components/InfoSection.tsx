import React from 'react';
import {
    makeStyles,
    Accordion,
    AccordionItem,
    AccordionHeader,
    AccordionPanel,
    tokens,
} from '@fluentui/react-components';


const AGENTS = [
    {
        id: 'orchestrator-agent',
        name: '🎯 Orchestrator Agent',
        description: 'Analyzes the user question and calls the right set of agents to answer the question properly.'
    },
    {
        id: 'visualization-agent',
        name: '📊 Visualization Agent',
        description: 'Generates graphs or charts to support the conversation and create clarity on installment amounts.'
    },
    {
        id: 'scenario-agent',
        name: '📅 Scenario Agent',
        description: 'Uses the MCP server to provide the user with next steps or recommendations based on the user\'s data.'
    },
    {
        id: 'rule-evaluation-agent',
        name: '📏 Rule Evaluation Agent',
        description: 'Evaluates the rules and policies that apply to the user\'s situation and provides guidance accordingly.'
    },
    {
        id: 'installment-agent',
        name: '💰 Update Installment Agent',
        description: 'Updates the installment amount for the user, taking into account the rules and policies defined by the company.'
    }
]
const useStyles = makeStyles({
    infoSection: {
        flexDirection: 'column',
        display: 'none',
        flex: 1,
        padding: '24px',
        backgroundColor: tokens.colorNeutralBackground3,
        borderRight: `1px solid ${tokens.colorNeutralStroke2}`,
        overflow: 'auto',
        width: '100%',
        // Tablet: show but still full width
        '@media (min-width: 768px)': {
            display: 'flex',
        },
        '@media (min-width: 1024px)': {
            display: 'flex',
            width: '350px',
            minWidth: '350px',
        },
    },
    infoTitle: {
        fontSize: tokens.fontSizeBase600,
        fontWeight: tokens.fontWeightSemibold,
        marginBottom: '16px',
        color: tokens.colorNeutralForeground1,
    },
    infoContent: {
        fontSize: tokens.fontSizeBase300,
        lineHeight: '1.5',
        color: tokens.colorNeutralForeground2,
        marginBottom: '24px',
    },
    accordion: {
        marginTop: '16px',
    },
    accordionPanel: {
        fontSize: tokens.fontSizeBase300,
        lineHeight: '1.4',
        color: tokens.colorNeutralForeground2,
    },
    accordionHeader: {
        fontWeight: tokens.fontWeightSemibold,
    },
});

export const InfoSection: React.FC = () => {
    const styles = useStyles();

    return (
        <div className={styles.infoSection}>
            <div className={styles.infoContent}>
                <p>
                    This sample demonstrates a multi-agent system that handles questions related to installment amounts. Some more details on the agents below:
                </p>
            </div>

            <Accordion className={styles.accordion} multiple collapsible defaultOpenItems={AGENTS.map(agent => agent.id)}>
                {AGENTS.map(agent => (
                    <AccordionItem key={agent.id} value={agent.id}>
                        <AccordionHeader className={styles.accordionHeader}>
                            {agent.name}
                        </AccordionHeader>
                        <AccordionPanel className={styles.accordionPanel}>
                            <p>{agent.description}</p>
                        </AccordionPanel>
                    </AccordionItem>
                ))}
            </Accordion>
        </div>
    );
};
