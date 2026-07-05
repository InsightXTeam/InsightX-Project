export interface Report {
  id: number;
  fileName: string;
  fileSize: number;
  uploadedAt: string;
  status: string;
  extractedText?: string;
  companyId?: number;
  uploadedById?: number;
}
