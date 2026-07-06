export interface Report {
  id: number;
  reportName: string;
  fileName: string;
  fileSize: number;
  uploadedAt: string;
  status: string;
  extractedText?: string;
  companyId?: number;
  uploadedById?: string;
  uploadedByName?: string;
}
