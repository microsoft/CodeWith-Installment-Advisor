import React from 'react';
import { createRoot } from 'react-dom/client';
import { FluentProvider, BrandVariants, createLightTheme, createDarkTheme, Theme } from '@fluentui/react-components';
import { App } from './App';
import './index.css';
import { useDarkMode } from 'hooks/useDarkMode';


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
  const { darkMode, toggleDarkMode } = useDarkMode();

  return (
    <FluentProvider theme={darkMode ? darkTheme : lightTheme}>
      <App darkMode={darkMode} toggleDarkMode={toggleDarkMode} />
    </FluentProvider>
  );
};

const container = document.getElementById('root');
if (!container) {
  throw new Error("Root container missing in index.html");
}
const root = createRoot(container);

root.render(<Root />);