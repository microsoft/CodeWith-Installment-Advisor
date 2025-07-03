import { ChatApiRequest, ChatApiResponse } from '../types';

const API_ENDPOINT = (process.env.REACT_APP_CHAT_API || 'https://localhost:55018').replace(/\/+$/, '') + '/chat';

export class ChatApiService {
  static async sendMessage(request: ChatApiRequest): Promise<ChatApiResponse | Response> {
    const response = await fetch(API_ENDPOINT, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    // If streaming is enabled, return the response directly for stream processing
    if (request.stream) {
      return response;
    }

    return await response.json();
  }
}
