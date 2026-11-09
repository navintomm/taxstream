export interface AiChatRequest {
  prompt: string;
}

export interface AiChatResponse {
  conversationId: string;
  response: string;
  disclaimer: string;
}

export interface AiHistoryItem {
  id: string;
  userPrompt: string;
  aiResponse: string;
  timestamp: string;
}

export interface AiHistoryResponse {
  items: AiHistoryItem[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
}
