import React from 'react';
import { createRoot } from 'react-dom/client';
import { FluentProvider, BrandVariants, createLightTheme, createDarkTheme, Theme } from '@fluentui/react-components';
import { App } from './App';
import './index.css';


// CHANGEME: theming of the app.
const demoTheme: BrandVariants = {
  10: "#020305",
  20: "#111723",
  30: "#16263D",
  40: "#193253",
  50: "#1B3F6A",
  60: "#1B4C82",
  70: "#18599B",
  80: "#1267B4",
  90: "#3174C2",
  100: "#4F82C8",
  110: "#6790CF",
  120: "#7D9ED5",
  130: "#92ACDC",
  140: "#A6BAE2",
  150: "#BAC9E9",
  160: "#CDD8EF"
};
const lightTheme: Theme = {
  ...createLightTheme(demoTheme),
};

const darkTheme: Theme = {
  ...createDarkTheme(demoTheme),
};


const Root: React.FC = () => {
  const [isDarkMode, setIsDarkMode] = React.useState(() => {
    const saved = localStorage.getItem('darkMode');
    return saved ? JSON.parse(saved) : false;
  });

  React.useEffect(() => {
    const handleStorageChange = () => {
      const saved = localStorage.getItem('darkMode');
      if (saved) {
        setIsDarkMode(JSON.parse(saved));
      }
    };

    window.addEventListener('storage', handleStorageChange);
    // Also listen for manual updates
    const checkInterval = setInterval(() => {
      const saved = localStorage.getItem('darkMode');
      if (saved) {
        const newValue = JSON.parse(saved);
        if (newValue !== isDarkMode) {
          setIsDarkMode(newValue);
        }
      }
    }, 100);

    return () => {
      window.removeEventListener('storage', handleStorageChange);
      clearInterval(checkInterval);
    };
  }, [isDarkMode]);

  return (
    <FluentProvider theme={isDarkMode ? darkTheme : lightTheme}>
      <App />
    </FluentProvider>
  );
};

const container = document.getElementById('root');
if (!container) {
  throw new Error("Root container missing in index.html");
}
const root = createRoot(container);

root.render(<Root />);