using CyberWare_With_ASM_PART2;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CyberWareASM
{
    
    public class ChatBot
    {
        // Injected dependencies
        private readonly KeywordResponder _keywords;
        private readonly TopicStore _topics;
        private readonly SentimentDetector _sentiment;
        private readonly MemoryStore _memory;
        private readonly Random _rng = new();

        // Conversation state
        private string _lastTopic = string.Empty;   // last matched keyword/topic

        // Fallback responses for unrecognised input
        private readonly List<string> _fallbacks = new()
        {
            "Hmm, I'm not sure how to answer that. Try asking about phishing, passwords, malware or VPNs!",
            "That's outside my current knowledge. Type 'help' for a list of topics I can assist with.",
            "I didn't quite catch that. Could you rephrase, or ask a cybersecurity-related question?",
            "I'm still learning! Try topics like encryption, firewalls, ransomware or data breaches.",
            "Not sure about that one. Ask me about social engineering, cloud security, or 2FA — I'll be more helpful!"
        };

        // Casual conversation responses
        private readonly Dictionary<string, List<string>> _casual = new()
        {
            ["how are you"] = new List<string>
            {
                "I'm doing great, thanks for asking! All systems are online and ready to help you stay secure. 🛡️",
                "Running at full capacity! No vulnerabilities detected in my circuits today. 😄 How can I help you?",
                "Fantastic! I'm here, alert and ready to tackle any cybersecurity question you throw at me. 🤖"
            },
            ["what is your name"] = new List<string>
            {
                "I'm CyberWare — your personal cybersecurity assistant, built with ASM! 🔐",
                "My name is CyberWare. I was created to keep you informed and secure in the digital world."
            },
            ["who are you"] = new List<string>
            {
                "I'm CyberWare — an AI-powered cybersecurity chatbot designed to educate and empower you online. 🤖🛡️",
                "I'm your digital security companion! Ask me anything about staying safe in the digital world."
            },
            ["what can you do"] = new List<string>
            {
                "I can answer questions on phishing, passwords, malware, privacy, scams, VPNs, encryption, firewalls and much more! Type 'help' for the full topic list. 🚀",
                "I detect your emotional tone and respond with empathy, remember your name and favourite topic, and give you tailored cybersecurity advice. Ask away!"
            },
            ["help"] = new List<string>
            {
                "Here's what I know about:\n• Phishing & Scams\n• Passwords & 2FA\n• Malware & Ransomware\n• Privacy & VPN\n• Firewalls & Network Security\n• Encryption & Cryptography\n• Social Engineering\n• Cloud Security\n• Data Breaches\n• Cyber Awareness\n\nJust type any topic and I'll help! 💬",
                "Type any cybersecurity topic and I'll respond! You can also say 'tell me more' to expand on the last topic we discussed. 🔐"
            },
            ["thank"] = new List<string>
            {
                "You're very welcome! Cybersecurity is everyone's responsibility — glad I could help. 🛡️",
                "My pleasure! Stay safe out there, and feel free to ask more anytime. 😊",
                "Anytime! Knowledge is your strongest defence. 💪"
            },
            ["bye"] = new List<string>
            {
                "Goodbye! Stay vigilant and secure out there! 👋🔐",
                "Take care! Remember — strong passwords, updated software, and healthy scepticism. 👋",
                "See you next time! Stay cyber-safe! 🛡️"
            },
            ["hello"] = new List<string>
            {
                "Hello! Great to have you here. What cybersecurity topic can I help you with today? 👋",
                "Hey there! CyberWare is ready. What would you like to learn about? 😊"
            },
            ["hi"] = new List<string>
            {
                "Hi! Ready to explore some cybersecurity knowledge? 🔐",
                "Hey! Ask me anything security-related and I'll do my best to help. 🤖"
            },
            ["good morning"] = new List<string>
            {
                "Good morning! Starting the day with some cybersecurity awareness is a great habit. ☀️",
                "Morning! Let's make today a secure one. What would you like to know? 🌅"
            },
            ["good afternoon"] = new List<string>
            {
                "Good afternoon! What cybersecurity topic are we exploring today? ☀️",
                "Afternoon! Ready to help you stay safe in the digital world. 🔐"
            },
            ["good evening"] = new List<string>
            {
                "Good evening! Stay safe online tonight. What can I help you with? 🌙",
                "Evening! Perfect time to brush up on some cyber hygiene. What do you need? 🌙"
            },
            ["purpose"] = new List<string>
            {
                "My purpose is to educate and empower you to stay safe in the digital world. Knowledge is your greatest defence! 🔐",
                "I was created to be your personal cybersecurity guide — helping you understand threats, best practices and how to stay protected online."
            },
            ["who created you"] = new List<string>
            {
                "I was built as part of the CyberWare With ASM project — a cybersecurity chatbot designed to make digital safety accessible to everyone. 🛡️",
                "I was created by ASM as an intelligent cybersecurity assistant. My mission is to make you safer online! 🤖"
            },
            ["interesting"] = new List<string>
            {
                "Cybersecurity is genuinely fascinating! The cat-and-mouse between attackers and defenders never stops evolving. 💡",
                "Right?! The more you learn about it, the more you realise how much there is to discover. Keep the curiosity going! 🔍"
            },
            ["okay"] = new List<string>
            {
                "Great! Feel free to ask me anything else. I'm here whenever you need. 😊",
                "Sounds good! What else can I help you with? 🔐"
            }
        };

        // Constructor

        public ChatBot(MemoryStore memory)
        {
            _memory = memory;
            _keywords = new KeywordResponder();
            _topics = new TopicStore();
            _sentiment = new SentimentDetector();
        }

        // Public API

        // Returns the personalised opening greeting shown immediately after login.
  
        public string GetGreeting()
        {
            return $"👋 Welcome, {_memory.FirstName}! I'm CyberWare — your intelligent cybersecurity assistant.\n\n" +
                   $"I'm here to help you navigate the digital world safely. You can ask me about:\n" +
                   $"• Phishing, Passwords, Malware, Privacy & Scams\n" +
                   $"• VPNs, Encryption, Firewalls & Network Security\n" +
                   $"• Social Engineering, Cloud Security & much more!\n\n" +
                   $"Type 'help' at any time to see all available topics. Let's get started, {_memory.FirstName}! 🛡️";
        }

        public string ProcessInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "Please type a message so I can help you! 😊";

            string lower = input.Trim().ToLowerInvariant();

            // 1. Follow-up handling
            if (IsFollowUp(lower))
            {
                if (!string.IsNullOrEmpty(_lastTopic))
                {
                    string? more = _keywords.GetResponse(_lastTopic)
                                   ?? _topics.GetResponse(_lastTopic);
                    if (more != null)
                        return $"Sure! Here's more on **{CapFirst(_lastTopic)}**:\n\n{more}\n\n💡 Say 'tell me more' again for another tip on this topic!";
                }
                return "I don't have a previous topic saved to expand on. What would you like to know more about? 🤔";
            }

            // 2. Casual conversation
            foreach (var kvp in _casual)
            {
                if (lower.Contains(kvp.Key))
                {
                    int idx = _rng.Next(kvp.Value.Count);
                    string response = kvp.Value[idx];
                    // Personalise with first name where possible
                    if (!lower.Contains("name") && _rng.Next(3) == 0)
                        response += $" (You're in safe hands, {_memory.FirstName} 😊)";
                    return response;
                }
            }

            // 3. Detect and store favourite topic
            DetectFavouriteTopic(lower);

            // 4. Sentiment detection
            var sentiment = _sentiment.Detect(lower);
            string opener = _sentiment.GetSentimentResponse(sentiment);

            // 5. Keyword lookup
            string? keywordResponse = _keywords.GetResponse(lower);
            if (keywordResponse != null)
            {
                SetLastTopic(lower, _keywords.GetAllKeywords());
                string personalised = _memory.GetPersonalisedOpener();
                return $"{opener}{personalised}{keywordResponse}\n\n💡 Type 'tell me more' for another tip on this topic!";
            }

            // 6. Topic store lookup
            string? topicResponse = _topics.GetResponse(lower);
            if (topicResponse != null)
            {
                SetLastTopic(lower, _topics.GetAllTopics());
                string personalised = _memory.GetPersonalisedOpener();
                return $"{opener}{personalised}{topicResponse}\n\n💡 Type 'tell me more' for another tip on this topic!";
            }

            // 7. Sentiment-only fallback
            if (sentiment != Sentiment.Neutral)
            {
                return $"{opener}I noticed something in your message. Here's a general tip, {_memory.FirstName}:\n\n" +
                       "🛡️ Always keep your software updated, use unique strong passwords for every account, " +
                       "enable 2FA where possible, and stay sceptical of unexpected messages. " +
                       "Feel free to ask about a specific topic for detailed advice!";
            }

            // 8. Random fallback
            return _fallbacks[_rng.Next(_fallbacks.Count)];
        }

        // Private helpers

        private static bool IsFollowUp(string lower)
            => lower.Contains("tell me more")
            || lower.Contains("explain more")
            || lower.Contains("more info")
            || lower.Contains("elaborate")
            || lower.Contains("expand on that")
            || lower.Contains("give me more");

        private void DetectFavouriteTopic(string lower)
        {
            var triggerPhrases = new[] { "interested in", "love", "favourite topic", "favorite topic", "i like", "i enjoy" };
            foreach (var phrase in triggerPhrases)
            {
                if (!lower.Contains(phrase)) continue;

                var allTopics = _keywords.GetAllKeywords().Concat(_topics.GetAllTopics());
                foreach (var topic in allTopics)
                {
                    if (lower.Contains(topic.ToLowerInvariant()))
                    {
                        _memory.FavouriteTopic = CapFirst(topic);
                        _lastTopic = topic;
                        return;
                    }
                }
            }
        }

        private void SetLastTopic(string lower, List<string> topicList)
        {
            foreach (var topic in topicList)
                if (lower.Contains(topic.ToLowerInvariant()))
                {
                    _lastTopic = topic;
                    return;
                }
        }

        private static string CapFirst(string s)
            => string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s[1..];
    }
}
