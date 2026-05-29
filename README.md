# CyberWare With ASM — Cybersecurity Chatbot

A WPF desktop chatbot application built in C# (.NET) for PROG6221 Part 2. The chatbot covers cybersecurity topics through keyword recognition, sentiment detection, memory recall, and voice greeting.


## Student Information

| Field           | Details                      |
|-----------------|------------------------------|
| Student Name    | Ahlumile Morris              |
| Student Number  | ST10488271                   |
| Module          | PROG6221 — Programming 2A    |
| Assessment      | Part 2 — WPF GUI Chatbot     |
| Lecturer        | Ms A Phewa                   |

## Project Overview

CyberWare With ASM is a WPF desktop chatbot that helps users learn about cybersecurity in a conversational way. On launch, the application asks for the user's name and surname, plays a personalised voice greeting (Ahlumile.wav), then opens the full chat interface. The chatbot remembers the user's name and favourite topic throughout the session, detects emotional sentiment in messages, and returns contextually relevant, randomly varied responses across 15 cybersecurity topics.


## Features Implemented

### GUI Design
- WPF layout with a blue, pink, and purple colour scheme
- ASCII art displayed in the header area
- Name and surname entry screen with input validation
- Voice greeting plays on startup using Ahlumile.wav
- Scrollable, read-only chat history panel
- Send button and Enter key both submit messages

### Keyword Recognition
15 cybersecurity keywords are recognised with dedicated response lists:
phishing, password, privacy, scam, malware, ransomware, firewall, encryption, vpn, two-factor authentication, social engineering, data breach, antivirus, dark web, zero-day.
Managed in KeywordResponder.cs using Dictionary<string, List<string>>.
Additional extended topics are stored in TopicStore.cs.

### Random Responses
Each keyword has a list of multiple responses. The Random class selects a different response on each match so the same response is never repeated consecutively.

### Conversation Flow
Supports follow-up phrases such as "tell me more", "explain more", and "elaborate". The bot continues on the last topic without resetting. Casual conversation is also supported: "how are you", "what can you do", "who are you".

### Memory and Recall
MemoryStore.cs stores the user's full name and favourite cybersecurity topic. The bot references the user's name throughout the conversation and uses the opener "As someone interested in [topic]..." when responding to related messages.

### Sentiment Detection
SentimentDetector.cs detects five sentiments: Worried, Curious, Frustrated, Happy, Neutral. After detecting a non-neutral sentiment, the bot automatically provides a cybersecurity tip without requiring another input from the user. An empathetic opening sentence is prepended to the tip.

### Code Optimisation
Logic is split across dedicated class files. MainWindow.xaml.cs handles only UI events and calls ChatBot.ProcessInput(). OOP principles are applied throughout: encapsulation, separation of concerns, use of dictionaries and enums.


## Project Structure

CyberWare_With_ASM/
|
|-- CyberWareWithASM.sln
|-- README.md
|
|-- CybersecurityChatbot/
|   |-- MainWindow.xaml
|   |-- MainWindow.xaml.cs
|   |-- ChatBot.cs
|   |-- KeywordResponder.cs
|   |-- SentimentDetector.cs
|   |-- MemoryStore.cs
|   |-- AudioPlayer.cs
|   |-- TopicStore.cs
|   |-- App.xaml
|   |-- App.xaml.cs
|   |-- CyberWareWithASM.csproj
|   `-- Ahlumile.wav
|
`-- .github/
    `-- workflows/
        `-- build.yml


## Screenshots
Chat Interface:

<img width="1919" height="1018" alt="Screenshot 2026-05-29 092156" src="https://github.com/user-attachments/assets/03de0ade-99c1-4516-8bd2-d40a53984d1f" />
