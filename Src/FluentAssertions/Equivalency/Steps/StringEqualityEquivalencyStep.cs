using System;
using FluentAssertions.Common;
using FluentAssertions.Execution;

namespace FluentAssertions.Equivalency.Steps;

public class StringEqualityEquivalencyStep : IEquivalencyStep
{
    public EquivalencyResult Handle(Comparands comparands, IEquivalencyValidationContext context,
        IValidateChildNodeEquivalency valueChildNodes)
    {
        Type expectationType = comparands.GetExpectedType(context.Options);

        if (expectationType is null || expectationType != typeof(string))
        {
            return EquivalencyResult.ContinueWithNext;
        }

        var assertionChain = AssertionChain.GetOrCreate().For(context);

        if (!ValidateAgainstNulls(assertionChain, comparands, context.CurrentNode))
        {
            return EquivalencyResult.EquivalencyProven;
        }

        bool subjectIsString = ValidateSubjectIsString(assertionChain, comparands, context.CurrentNode);

        if (subjectIsString)
        {
            string subject = (string)comparands.Subject;
            string expectation = (string)comparands.Expectation;

            assertionChain.ReuseOnce();
            subject.Should()
                .Be(expectation, CreateOptions(context.Options), context.Reason.FormattedMessage, context.Reason.Arguments);
        }

        return EquivalencyResult.EquivalencyProven;
    }

    private static Func<EquivalencyOptions<string>, EquivalencyOptions<string>>
        CreateOptions(IEquivalencyOptions existingOptions) =>
        o =>
        {
            if (existingOptions is EquivalencyOptions<string> equivalencyOptions)
            {
                return equivalencyOptions;
            }

            if (existingOptions.IgnoreLeadingWhitespace)
            {
                o.IgnoringLeadingWhitespace();
            }

            if (existingOptions.IgnoreTrailingWhitespace)
            {
                o.IgnoringTrailingWhitespace();
            }

            if (existingOptions.IgnoreCase)
            {
                o.IgnoringCase();
            }

            if (existingOptions.IgnoreNewlineStyle)
            {
                o.IgnoringNewlineStyle();
            }

            return o;
        };

    private static bool ValidateAgainstNulls(AssertionChain assertionChain, Comparands comparands, INode currentNode)
    {
        object expected = comparands.Expectation;
        object subject = comparands.Subject;

        bool onlyOneNull = expected is null != subject is null;

        if (onlyOneNull)
        {
            assertionChain.FailWith(
                $"Expected {currentNode.Subject.Description.EscapePlaceholders()} to be {{0}}{{reason}}, but found {{1}}.", expected, subject);

            return false;
        }

        return true;
    }

    private static bool ValidateSubjectIsString(AssertionChain assertionChain, Comparands comparands, INode currentNode)
    {
        if (comparands.Subject is string)
        {
            return true;
        }

        assertionChain.FailWith(
            $"Expected {currentNode} to be {{0}}, but found {{1}}.",
            comparands.RuntimeType, comparands.Subject.GetType());

        return assertionChain.Succeeded;
    }
}
