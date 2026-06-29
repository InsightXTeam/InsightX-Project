# InsightX-Project - Report Upload & AI Processing Module

InsightX Project is a SaaS platform that uses Generative AI, LLMs, and vector search to analyze business reports, extract actionable insights, generate real-time alerts, and provide an intelligent chat assistant for data-driven decision-making.

**Note:** This branch (`feature/report-upload`) is isolated to contain **only the Report Upload & AI Processing Module** (Person 2's scope). Features like Authentication, User Management, and RAG Chat have been intentionally removed from this branch for isolated development and testing.

## Features Included in this Branch:
- **Report Upload**: Users can upload business reports (PDF, Word, Images, Excel).
- **Document Processing**: Asynchronous extraction of text and semantic data using Semantic Kernel and Tesseract OCR.
- **Metric Extraction**: Automatic extraction of actionable business metrics from the parsed documents using AI.
- **Report Status & Preview**: Endpoints to track the processing status and preview the extracted data.
