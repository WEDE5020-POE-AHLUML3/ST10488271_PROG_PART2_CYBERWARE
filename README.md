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
