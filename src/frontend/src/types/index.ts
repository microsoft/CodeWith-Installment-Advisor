export interface ChatMessage {
  sender: 'user' | 'bot';
  text: string;
  images?: string[];
  isTyping?: boolean;
}

export interface ScenarioCard {
  text: string;
}

export interface ChatApiRequest {
  UserID: string;
  message: string;
  threadId?: string;
}

export interface ChatApiResponse {
  reply?: string;
  message?: string;
  threadId?: string;
  images?: string[];
}
