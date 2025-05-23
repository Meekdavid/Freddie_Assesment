# 🧠⚡ Freddie Recruit AI Rating System
  
  *"Precision candidate evaluation powered by AI and Google Workspace"*
  
  [![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
  [![Live Demo](https://img.shields.io/badge/Demo-Live-green)](https://freddierecruit.runasp.net/api/rankings)
  [![Swagger Docs](https://img.shields.io/badge/Docs-Swagger-orange)](https://freddierecruit.runasp.net/swagger/index.html)
</div>

## 🌟 Objective?

Transforms recruitment by combining AI analysis with seamless Google Workspace integration to deliver:

```diff
+✅ Faster candidate screening
+🎯 Data-driven hiring decisions
+📈 Consistent evaluation metrics
```

---

## 🛠️ Core Technology Stack

| Component              | Technology               | Purpose                          |
|------------------------|--------------------------|----------------------------------|
| **Backbone**           | .NET Core 8.0            | High-performance processing      |
| **Data Storage**       | SQLite                   | Local candidate database         |
| **Cloud Integration**  | Google Sheets/Drive API  | Resume & candidate data pipeline |
| **AI Engine**          | OpenAI GPT-4             | Intelligent candidate analysis   |
| **Delivery**           | REST API                 | Seamless integration             |

---

## 🔄 Detailed Workflow Process

### 1. 📥 Fetch Applicant Data
```mermaid
graph LR
    A[Google Sheets] -->|Spreadsheet ID| B[Retrieve Candidate Details]
    B --> C[Extract: Name, Email, Resume URL]
```

- Connects to Google Sheets using the provided Spreadsheet ID
- Retrieves all applicant details including resume URLs

### 2. 📄 Process Resume Content
```mermaid
graph LR
    D[Resume URL] --> E[Google Drive API]
    E --> F[Download PDF]
    F --> G[Extract Text]
```

- Accesses each candidate's resume via Google Drive
- Downloads and extracts text content from PDFs
- Formats resume content for AI processing

### 3. 🔍 Check Existing Evaluations
```mermaid
graph TD
    H[SQLite Database] --> I{Exists?}
    I -->|Yes| J[Skip Re-evaluation]
    I -->|No| K[Proceed to AI Analysis]
```

- Verifies if candidate already exists in local SQLite database
- Prevents duplicate processing of previously evaluated candidates

### 4. 🤖 AI-Powered Evaluation
```mermaid
graph LR
    L[Structured Prompt] --> M[OpenAI API]
    M --> N[JSON Response]
    N --> O[Parse Rating]
```

**Sample Prompt Structure:**
```text
Analyze this candidate for [Position]:
- Name: [Candidate Name]
- Strengths: [Key Strengths]
- Weakness: [Biggest Weakness]
- Available: [Yes/No]
Resume Highlights: [Formatted Content]

Evaluation Criteria:
1. Relevant Experience
2. Skills Match
3. Cultural Fit

Response Format: {"rate":0-100,"details":"analysis"}
```

### 5. 💾 Store Evaluation Results
```mermaid
graph LR
    P[AI Results] --> Q[SQLite Database]
    Q --> R[Persistent Storage]
```

- Saves complete evaluation to `Freddie.db`
- Includes rating, analysis notes, and evaluation timestamp

### 6. 📊 Update Google Sheets
```mermaid
graph LR
    S[Processed Data] --> T[New Spreadsheet]
    T --> U[Shareable Results]
```

- Generates new Google Spreadsheet with all evaluations
- Returns spreadsheet URL in API response

---

## 🚀 Live System Access

<div align="center">
  <a href="https://freddierecruit.runasp.net/api/rankings">
    <img src="https://img.shields.io/badge/ACCESS%20THE%20API-HERE-brightgreen?style=for-the-badge" alt="API Access">
  </a>
  
  <a href="https://freddierecruit.runasp.net/swagger/index.html">
    <img src="https://img.shields.io/badge/VIEW%20DOCS-SWAGGER-orange?style=for-the-badge" alt="Swagger Docs">
  </a>
</div>

---

## 🧩 System Components

| Service File               | Purpose                                                                 | Key Methods                     |
|----------------------------|-------------------------------------------------------------------------|---------------------------------|
| `GoogleSheetsService.cs`   | Handles all Google Sheets interactions                                  | `GetCandidatesAsync()`          |
| `ResumeProcessor.cs`       | Manages resume downloads and text extraction                           | `ExtractResumeTextAsync()`      |
| `OpenAiEvaluationService.cs` | Orchestrates AI analysis and rating generation                        | `EvaluateCandidateAsync()`      |
| `RecruitmentProcessor.cs`  | Main workflow coordinator                                              | `RunEvaluationPipelineAsync()`  |
| `GoogleAuthService.cs`     | Centralized Google API authentication                                  | `GetCredentials()`              |

---

## 📋 Sample Outputs

### AI Response Example
```json
{
  "rate": 85,
  "details": "Candidate demonstrates strong experience in digital marketing with excellent technical skills..."
}
```

### API Final Response
```json
{
  "status": "Success",
  "spreadsheet": "https://docs.google.com/spreadsheets/d/ABC123",
  "evaluations": [
    {
      "candidate": "Samantha Greene",
      "score": 85,
      "analysis": "6 years relevant experience...",
      "date": "2025-03-29T14:30:00Z"
    }
  ]
}
```

---

## 🛡️ Security Architecture

```mermaid
graph TD
    A[Environment Config] --> B[Encrypted Creds]
    B --> C[OAuth 2.0]
    C --> D[API Access]
    D --> E[Audit Logging]
```

- **Multi-layer protection** for API keys and credentials
- **Strict scope limitations** on Google API permissions
- **Automated token refresh** for continuous security

---

## 🚀 Getting Started

### Prerequisites
- .NET 8.0 SDK. Install the sdk from https://dotnet.microsoft.com/en-us/download/dotnet/8.0
- Google Cloud Project
- OpenAI API access

### Configuration
1. Download the published version of the code from https://drive.google.com/drive/folders/1B5Sj8UyEhnUEnQiWaF4aoduqkB2pDxOR
2. Amend `appsettings.Production.json` from the downloaded file with the correct credentials, after the amendment is done, execute the application 'Freddie.exe'
   ```json
   {
   "ApplicationSettings": {
    "EmailDetails": {
      "SMTPServer": "smtp.gmail.com",
      "SenderEmail": "mbokodavid@gmail.com",
      "Port": 587, // Use 465 for SSL, 587 for TLS
      "UseSsl": true,
      "MemberName": "", // Not relevant
      "Password": "", // Not relevant
      "APIKey": "" // Openapi API key

    },
    "FireBaseStorage": {
      "BucketName": "smtp.gmail.com",
      "Port": 587, // Use 465 for SSL, 587 for TLS
      "BaseUrl": "https://firebasestorage.googleapis.com/v0/b/"
    },
    "JwtConfig": {
      "Issuer": "https://localhost",
      "Audience": "https://localhost",
      "SecretKey": "",
      "ExpiryDate": 120 // Minutes before token expiration
    },
    "ServiceAccountKeyPath": "", // The json file name for google api. Ensure this file is added to the folder downloaded above
    "JobRoleToEvaluateCandidate": "Marketing Officer",
    "CandidateEligibilityThreshold": 10,
    "MaximumTokenEdebAI": "150",
    "OpenAITemperature": "0.8",
    "FreddieGmail": "mbokodavid@gmail.com",
    "SpreadsheetId": "", // The spreadsheet Id to retrieve candidate data from
    "Key": "",
    "OpenAIKey": "" // Openapi API key
  }
}
```
```
---

<div align="center">
  <h3>💡 Intelligent Hiring Starts Here</h3>
  <p>
    <a href="#-freddie-recruit-ai-rating-system">Back to Top</a> •
    <a href="CONTRIBUTING.md">Contribute</a> •
    <a href="LICENSE">License</a>
  </p>
</div>
