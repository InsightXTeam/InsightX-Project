using System.Collections.Generic;
using System.Linq;
using InsightX.Application.Interfaces;

namespace InsightX.Application.Common
{
    public static class AgentPromptBuilder
    {
        public const string SystemPromptTemplate =
            "You are InsightX AI, a business intelligence assistant. You ONLY answer based " +
            "on the context provided below. If the answer is not in the context, say: \"I do " +
            "not have enough data to answer this.\" Never invent numbers or facts. Always " +
            "mention which report or month your answer comes from. " +
            "Context from company reports: {retrieved_chunks} " +
            "Question: {user_question}";

        public static string BuildPrompt(string question, IReadOnlyList<RagChunk> chunks)
        {
            var contextText = string.Join(
                "\n---\n",
                chunks.Select(c => $"[Report ID: {c.ReportId}, Month: {c.Month}, Year: {c.Year}]: {c.Text}"));

            return SystemPromptTemplate
                .Replace("{retrieved_chunks}", contextText)
                .Replace("{user_question}", question);
        }
    }
}
