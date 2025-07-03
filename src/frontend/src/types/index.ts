export interface ChatMessage {
  sender: 'user' | 'bot';
  text: string;
  images?: string[];
  isTyping?: boolean;
  typingInfo?: string;
}

export interface ScenarioCard {
  text: string;
}

export interface ChatApiRequest {
  UserID: string;
  message: string;
  threadId?: string;
  stream?: boolean;
}

export interface ChatApiResponse {
  reply?: string;
  message?: string;
  threadId?: string;
  images?: string[];
}
