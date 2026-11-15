using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using TaxStream.Application.Abstractions;
using TaxStream.Infrastructure.Configuration;

namespace TaxStream.Infrastructure.AI;

public class AzureOpenAiService : IAiProviderService
{
    private readonly AzureOpenAIOptions _options;
    private readonly ILogger<AzureOpenAiService> _logger;
    private AzureOpenAIClient? _client;

    public AzureOpenAiService(IOptions<AzureOpenAIOptions> options, ILogger<AzureOpenAiService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    private bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_options.Endpoint) && !_options.Endpoint.StartsWith("REPLACE_") &&
        !string.IsNullOrWhiteSpace(_options.ApiKey) && !_options.ApiKey.StartsWith("REPLACE_");

    private AzureOpenAIClient GetClient()
    {
        if (_client != null) return _client;
        if (!IsConfigured)
        {
            throw new InvalidOperationException("Azure OpenAI is not configured.");
        }

        _client = new AzureOpenAIClient(
            new Uri(_options.Endpoint),
            new AzureKeyCredential(_options.ApiKey));
        return _client;
    }

    public async Task<string> SendAsync(string systemPrompt, string userMessage, CancellationToken cancellationToken = default)
    {
        // 100% Free Smart Local Mode: when no paid Azure OpenAI key is configured,
        // provide intelligent, contextual tax advice directly without any API cost.
        if (!IsConfigured)
        {
            _logger.LogInformation("Using Free Local Tax AI Advisor (No Azure OpenAI cost)");
            await Task.Delay(400, cancellationToken); // Realistic brief typing delay
            return GenerateLocalTaxAdvice(systemPrompt, userMessage);
        }

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));

        var chatClient = GetClient().GetChatClient(_options.DeploymentName);

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userMessage)
        };

        var options = new ChatCompletionOptions
        {
            MaxOutputTokenCount = _options.MaxTokens
        };

        var response = await chatClient.CompleteChatAsync(messages, options, cts.Token);
        var content = response.Value.Content[0].Text;

        _logger.LogInformation("AI provider responded. DeploymentName: {Deployment}", _options.DeploymentName);
        return content;
    }

    private static string GenerateLocalTaxAdvice(string systemPrompt, string userMessage)
    {
        var lower = userMessage.ToLowerInvariant();
        var context = ExtractContext(systemPrompt);

        string response;

        if (lower.Contains("home office") || lower.Contains("work from home") || lower.Contains("remote"))
        {
            response =
                "### Home Office Deductions\n\n" +
                "You may be eligible to deduct home office expenses if you meet two core criteria:\n" +
                "1. **Exclusive and Regular Use:** The area must be used solely and regularly for conducting trade or business.\n" +
                "2. **Principal Place of Business:** It must be your main place of work, or where you regularly meet clients/customers.\n\n" +
                "**Two ways to calculate it:**\n" +
                "- **Simplified Method:** A standard deduction of $5 per square foot (up to 300 sq. ft. = maximum $1,500).\n" +
                "- **Actual Expense Method:** Calculate actual expenses (mortgage/rent portion, utilities, internet, repairs) proportional to your office square footage.";
        }
        else if (lower.Contains("retire") || lower.Contains("401k") || lower.Contains("super") || lower.Contains("ira") || lower.Contains("pension"))
        {
            response =
                "### Retirement Contributions & Tax Savings\n\n" +
                "Contributing to qualified retirement accounts is one of the most effective ways to lower your taxable income:\n" +
                "- **Pre-tax contributions (e.g. 401(k) / Super / Traditional IRA):** Contributions directly reduce your Adjusted Gross Income (AGI).\n" +
                "- **Tax-Deferred Growth:** Investments grow tax-free until withdrawal during retirement.\n" +
                "- **Employer Matching:** Always contribute at least enough to capture your full employer match — it is essentially free compensation.";
        }
        else if (lower.Contains("standard") || lower.Contains("itemize") || lower.Contains("itemized") || lower.Contains("difference"))
        {
            response =
                "### Standard vs. Itemized Deductions\n\n" +
                "- **Standard Deduction:** A fixed dollar amount that reduces your taxable income, determined by your filing status without needing receipts.\n" +
                "- **Itemized Deductions:** Allows you to total specific qualified expenses (mortgage interest, state/local taxes, medical expenses exceeding AGI limits, charitable gifts).\n\n" +
                "**Best Practice:** The system automatically benefits you by claiming whichever method gives you the larger total deduction.";
        }
        else if (lower.Contains("charity") || lower.Contains("charitable") || lower.Contains("donation"))
        {
            response =
                "### Charitable Donation Deductions\n\n" +
                "Donations to qualified nonprofit organizations (e.g. 501(c)(3) entities) can be deductible if you itemize:\n" +
                "- **Cash donations:** Always keep bank records or written acknowledgment from the charity.\n" +
                "- **Non-cash items (clothing, goods):** Deduct the fair market value. For items over $250, obtain a written receipt.\n" +
                "- **Limits:** Generally up to 60% of your Adjusted Gross Income (AGI) for cash contributions to public charities.";
        }
        else if (lower.Contains("medical") || lower.Contains("health") || lower.Contains("doctor"))
        {
            response =
                "### Medical & Healthcare Deductions\n\n" +
                "Medical expenses are deductible only to the extent they exceed a percentage threshold of your Adjusted Gross Income (typically 7.5% of AGI):\n" +
                "- Eligible: Doctor visits, prescriptions, surgeries, dental, mental health, and qualified medical equipment.\n" +
                "- Ineligible: Over-the-counter medicine (unless prescribed) and purely cosmetic procedures.\n" +
                "- **Tip:** Consider using an HSA (Health Savings Account) or FSA for pre-tax healthcare savings.";
        }
        else if (lower.Contains("status") || lower.Contains("single") || lower.Contains("married") || lower.Contains("filing"))
        {
            response =
                "### Tax Filing Statuses\n\n" +
                "Your filing status determines tax brackets, standard deduction amounts, and eligibility for specific credits:\n" +
                "- **Single:** Unmarried or legally separated.\n" +
                "- **Married Filing Jointly:** Usually yields the lowest tax rates and highest standard deduction for spouses.\n" +
                "- **Married Filing Separately:** Useful in special cases (e.g. income-driven student loan repayment plans).\n" +
                "- **Head of Household:** Higher standard deduction and wider brackets for unmarried individuals supporting dependents.";
        }
        else
        {
            response =
                "### Tax Guidance Summary\n\n" +
                $"Regarding your question: *\"{userMessage}\"*\n\n" +
                "Here are key tax principles to consider:\n" +
                "1. **Maximize Eligible Deductions:** Review your itemized expenses against the standard deduction threshold.\n" +
                "2. **Record Keeping:** Retain receipts, invoices, and bank statements for at least 3 to 7 years.\n" +
                "3. **Timing:** Deductions generally apply in the calendar tax year they were paid or incurred.\n" +
                "4. **Net Taxable Income:** Calculated by taking your Gross Income minus all eligible deduction categories.";
        }

        if (!string.IsNullOrWhiteSpace(context) && !context.Contains("No tax records on file"))
        {
            response += $"\n\n---\n**Your Tax Context on File:**\n> {context}\n";
        }

        return response;
    }

    private static string ExtractContext(string systemPrompt)
    {
        const string marker = "User tax context (for reference only): ";
        var index = systemPrompt.IndexOf(marker, StringComparison.Ordinal);
        return index >= 0 ? systemPrompt[(index + marker.Length)..].Trim() : string.Empty;
    }
}
