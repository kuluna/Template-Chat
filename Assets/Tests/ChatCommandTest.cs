using NUnit.Framework;

#nullable enable

public class ChatCommandTest
{
    public class Text
    {
        [Test]
        public void Parseable()
        {
            var text = "@text, Hello, world!";
            var command = new TextChatCommand(0, text.Split(','));

            Assert.AreEqual(CommandType.Text, command.Type);
            Assert.AreEqual("Hello, world!", command.Text);
        }

        [Test]
        public void InvalidTextCommand_ThrowsException()
        {
            var text = "@text"; // Missing text argument
            Assert.Throws<ChatCommandException>(() =>
            {
                var command = new TextChatCommand(0, text.Split(','));
                Assert.Fail("Expected ChatCommandException was not thrown.");
            });
        }

        [Test]
        public void InvalidTextCommand_EmptyText()
        {
            var text = "@text,   "; // Text argument is empty after trimming
            Assert.Throws<ChatCommandException>(() =>
            {
                var command = new TextChatCommand(0, text.Split(','));
                Assert.Fail("Expected ChatCommandException was not thrown.");
            });
        }
    }

    public class Image
    {
        [Test]
        public void Parseable()
        {
            var text = "@image, MyImage";
            var command = new ImageChatCommand(0, text.Split(','));

            Assert.AreEqual(CommandType.Image, command.Type);
            Assert.AreEqual("MyImage", command.ImageName);
        }

        [Test]
        public void InvalidImageCommand_MissingArgument()
        {
            var text = "@image"; // Missing image name argument
            Assert.Throws<ChatCommandException>(() =>
            {
                var command = new ImageChatCommand(0, text.Split(','));
                Assert.Fail("Expected ChatCommandException was not thrown.");
            });
        }

        [Test]
        public void InvalidImageCommand_EmptyImageName()
        {
            var text = "@image,   "; // Image name is empty after trimming
            Assert.Throws<ChatCommandException>(() =>
            {
                var command = new ImageChatCommand(0, text.Split(','));
                Assert.Fail("Expected ChatCommandException was not thrown.");
            });
        }

        [Test]
        public void InvalidImageCommand_TooManyArguments()
        {
            var text = "@image, MyImage, Extra"; // Too many arguments
            Assert.Throws<ChatCommandException>(() =>
            {
                var command = new ImageChatCommand(0, text.Split(','));
                Assert.Fail("Expected ChatCommandException was not thrown.");
            });
        }
    }

    public class Choice
    {
        [Test]
        public void Parseable()
        {
            var text = "@choice, user_choice, Option1, Option2";
            var command = new ChoiceChatCommand(0, text.Split(','));

            Assert.AreEqual(CommandType.Choice, command.Type);
            Assert.AreEqual("user_choice", command.VariableName);
            Assert.AreEqual(2, command.Choices.Length);
            Assert.AreEqual("Option1", command.Choices[0]);
            Assert.AreEqual("Option2", command.Choices[1]);
        }

        [Test]
        public void Parseable_OneChoice()
        {
            var text = "@choice, my_var, Option1";
            var command = new ChoiceChatCommand(0, text.Split(','));

            Assert.AreEqual(CommandType.Choice, command.Type);
            Assert.AreEqual("my_var", command.VariableName);
            Assert.AreEqual(1, command.Choices.Length);
            Assert.AreEqual("Option1", command.Choices[0]);
        }

        [Test]
        public void Parseable_ThreeChoices()
        {
            var text = "@choice, my_var, Option1, Option2, Option3";
            var command = new ChoiceChatCommand(0, text.Split(','));

            Assert.AreEqual(CommandType.Choice, command.Type);
            Assert.AreEqual("my_var", command.VariableName);
            Assert.AreEqual(3, command.Choices.Length);
            Assert.AreEqual("Option1", command.Choices[0]);
            Assert.AreEqual("Option2", command.Choices[1]);
            Assert.AreEqual("Option3", command.Choices[2]);
        }

        [Test]
        public void InvalidChoiceCommand_MissingArgument()
        {
            var text = "@choice"; // Missing variable name and choices
            Assert.Throws<ChatCommandException>(() =>
            {
                var command = new ChoiceChatCommand(0, text.Split(','));
                Assert.Fail("Expected ChatCommandException was not thrown.");
            });
        }

        [Test]
        public void InvalidChoiceCommand_MissingChoices()
        {
            var text = "@choice, var_name"; // Missing choice arguments
            Assert.Throws<ChatCommandException>(() =>
            {
                var command = new ChoiceChatCommand(0, text.Split(','));
                Assert.Fail("Expected ChatCommandException was not thrown.");
            });
        }

        [Test]
        public void InvalidChoiceCommand_TooManyChoices()
        {
            var text = "@choice, var_name, Option1, Option2, Option3, Option4"; // More than 3 choices
            Assert.Throws<ChatCommandException>(() =>
            {
                var command = new ChoiceChatCommand(0, text.Split(','));
                Assert.Fail("Expected ChatCommandException was not thrown.");
            });
        }

        [Test]
        public void InvalidChoiceCommand_EmptyVariableName()
        {
            var text = "@choice,   , Option1, Option2"; // Variable name is empty after trimming
            Assert.Throws<ChatCommandException>(() =>
            {
                var command = new ChoiceChatCommand(0, text.Split(','));
                Assert.Fail("Expected ChatCommandException was not thrown.");
            });
        }

        [Test]
        public void InvalidChoiceCommand_EmptyChoice()
        {
            var text = "@choice, var_name, Option1,   "; // Second choice is empty after trimming
            Assert.Throws<ChatCommandException>(() =>
            {
                var command = new ChoiceChatCommand(0, text.Split(','));
                Assert.Fail("Expected ChatCommandException was not thrown.");
            });
        }

        [Test]
        public void InvalidChoiceCommand_EmptyFirstChoice()
        {
            var text = "@choice, var_name,   , Option2"; // First choice is empty after trimming
            Assert.Throws<ChatCommandException>(() =>
            {
                var command = new ChoiceChatCommand(0, text.Split(','));
                Assert.Fail("Expected ChatCommandException was not thrown.");
            });
        }
    }

    public class If
    {
        [Test]
        public void Parseable_StringComparison()
        {
            var text = "@if, playerName, Alice, NextScene";
            var command = new IfChatCommand(0, text.Split(','));

            Assert.AreEqual(CommandType.If, command.Type);
            Assert.AreEqual("playerName", command.VariableName);
            Assert.AreEqual("Alice", command.ExpectedValue);
            Assert.AreEqual("NextScene", command.GotoLabel);
            Assert.AreEqual(IfChatCommand.EvalType.String, command.ValueEvalType);
        }

        [Test]
        public void Parseable_NumericComparison()
        {
            var text = "@if, score, 100, WinLabel";
            var command = new IfChatCommand(0, text.Split(','));

            Assert.AreEqual(CommandType.If, command.Type);
            Assert.AreEqual("score", command.VariableName);
            Assert.AreEqual("100", command.ExpectedValue);
            Assert.AreEqual("WinLabel", command.GotoLabel);
            Assert.AreEqual(IfChatCommand.EvalType.Numeric, command.ValueEvalType);
        }

        [Test]
        public void Parseable_NumericComparison_Equal()
        {
            var text = "@if, score, =100, WinLabel";
            var command = new IfChatCommand(0, text.Split(','));

            Assert.AreEqual(CommandType.If, command.Type);
            Assert.AreEqual("score", command.VariableName);
            Assert.AreEqual("=100", command.ExpectedValue);
            Assert.AreEqual("WinLabel", command.GotoLabel);
            Assert.AreEqual(IfChatCommand.EvalType.Numeric, command.ValueEvalType);
        }

        [Test]
        public void Parseable_NumericComparison_GreaterThan()
        {
            var text = "@if, score, >20, WinLabel";
            var command = new IfChatCommand(0, text.Split(','));

            Assert.AreEqual(CommandType.If, command.Type);
            Assert.AreEqual("score", command.VariableName);
            Assert.AreEqual(">20", command.ExpectedValue);
            Assert.AreEqual("WinLabel", command.GotoLabel);
            Assert.AreEqual(IfChatCommand.EvalType.Numeric, command.ValueEvalType);
        }

        [Test]
        public void Parseable_NumericComparison_LessThan()
        {
            var text = "@if, score, <20, LoseLabel";
            var command = new IfChatCommand(0, text.Split(','));

            Assert.AreEqual(CommandType.If, command.Type);
            Assert.AreEqual("score", command.VariableName);
            Assert.AreEqual("<20", command.ExpectedValue);
            Assert.AreEqual("LoseLabel", command.GotoLabel);
            Assert.AreEqual(IfChatCommand.EvalType.Numeric, command.ValueEvalType);
        }

        [Test]
        public void Parseable_BooleanComparison()
        {
            var text = "@if, hasKey, true, OpenDoor";
            var command = new IfChatCommand(0, text.Split(','));

            Assert.AreEqual(CommandType.If, command.Type);
            Assert.AreEqual("hasKey", command.VariableName);
            Assert.AreEqual("true", command.ExpectedValue);
            Assert.AreEqual("OpenDoor", command.GotoLabel);
            Assert.AreEqual(IfChatCommand.EvalType.Boolean, command.ValueEvalType);
        }

        [Test]
        public void InvalidIfCommand_MissingArguments()
        {
            var text = "@if, varName"; // Missing expected value and goto label
            Assert.Throws<ChatCommandException>(() =>
            {
                var command = new IfChatCommand(0, text.Split(','));
                Assert.Fail("Expected ChatCommandException was not thrown.");
            });
        }

        [Test]
        public void InvalidIfCommand_TooManyArguments()
        {
            var text = "@if, varName, value, label, extra"; // Too many arguments
            Assert.Throws<ChatCommandException>(() =>
            {
                var command = new IfChatCommand(0, text.Split(','));
                Assert.Fail("Expected ChatCommandException was not thrown.");
            });
        }

        [Test]
        public void InvalidIfCommand_EmptyVariableName()
        {
            var text = "@if,   , value, label"; // Variable name is empty
            Assert.Throws<ChatCommandException>(() =>
            {
                var command = new IfChatCommand(0, text.Split(','));
                Assert.Fail("Expected ChatCommandException was not thrown.");
            });
        }

        [Test]
        public void InvalidIfCommand_EmptyExpectedValue()
        {
            var text = "@if, varName,   , label"; // Expected value is empty
            Assert.Throws<ChatCommandException>(() =>
            {
                var command = new IfChatCommand(0, text.Split(','));
                Assert.Fail("Expected ChatCommandException was not thrown.");
            });
        }

        [Test]
        public void InvalidIfCommand_EmptyGotoLabel()
        {
            var text = "@if, varName, value,   "; // Goto label is empty
            Assert.Throws<ChatCommandException>(() =>
            {
                var command = new IfChatCommand(0, text.Split(','));
                Assert.Fail("Expected ChatCommandException was not thrown.");
            });
        }

        [Test]
        public void InvalidIfCommand_InvalidNumericValue()
        {
            var text = "@if, score, >abc, label"; // Invalid numeric value
            Assert.Throws<ChatCommandException>(() =>
            {
                var command = new IfChatCommand(0, text.Split(','));
                Assert.Fail("Expected ChatCommandException was not thrown.");
            });
        }

        [Test]
        public void InvalidIfCommand_InvalidNumericValue_Equal()
        {
            var text = "@if, score, =1xyz, label"; // Invalid numeric value with =
            Assert.Throws<ChatCommandException>(() =>
            {
                var command = new IfChatCommand(0, text.Split(','));
                Assert.Fail("Expected ChatCommandException was not thrown.");
            });
        }

        [Test]
        public void InvalidIfCommand_InvalidNumericValue_LessThan()
        {
            var text = "@if, score, <notanumber, label"; // Invalid numeric value with <
            Assert.Throws<ChatCommandException>(() =>
            {
                var command = new IfChatCommand(0, text.Split(','));
                Assert.Fail("Expected ChatCommandException was not thrown.");
            });
        }
    }

    public class Label
    {
        [Test]
        public void Parseable()
        {
            var text = "@label, StartPoint";
            var command = new LabelChatCommand(0, text.Split(','));

            Assert.AreEqual(CommandType.Label, command.Type);
            Assert.AreEqual("StartPoint", command.LabelName);
        }

        [Test]
        public void InvalidLabelCommand_MissingArgument()
        {
            var text = "@label"; // Missing label name argument
            Assert.Throws<ChatCommandException>(() =>
            {
                var command = new LabelChatCommand(0, text.Split(','));
                Assert.Fail("Expected ChatCommandException was not thrown.");
            });
        }

        [Test]
        public void InvalidLabelCommand_EmptyLabelName()
        {
            var text = "@label,   "; // Label name is empty after trimming
            Assert.Throws<ChatCommandException>(() =>
            {
                var command = new LabelChatCommand(0, text.Split(','));
                Assert.Fail("Expected ChatCommandException was not thrown.");
            });
        }

        [Test]
        public void InvalidLabelCommand_TooManyArguments()
        {
            var text = "@label, StartPoint, Extra"; // Too many arguments
            Assert.Throws<ChatCommandException>(() =>
            {
                var command = new LabelChatCommand(0, text.Split(','));
                Assert.Fail("Expected ChatCommandException was not thrown.");
            });
        }
    }

    public class Wait
    {
        [Test]
        public void Parseable()
        {
            var text = "@wait, 5";
            var command = new WaitChatCommand(0, text.Split(','));

            Assert.AreEqual(CommandType.Wait, command.Type);
            Assert.AreEqual(5f, command.Seconds);
        }

        [Test]
        public void InvalidWaitCommand_MissingArgument()
        {
            var text = "@wait"; // Missing seconds argument
            Assert.Throws<ChatCommandException>(() =>
            {
                var command = new WaitChatCommand(0, text.Split(','));
                Assert.Fail("Expected ChatCommandException was not thrown.");
            });
        }

        [Test]
        public void InvalidWaitCommand_InvalidNumber()
        {
            var text = "@wait, abc"; // Invalid number format
            Assert.Throws<ChatCommandException>(() =>
            {
                var command = new WaitChatCommand(0, text.Split(','));
                Assert.Fail("Expected ChatCommandException was not thrown.");
            });
        }

        [Test]
        public void InvalidWaitCommand_NegativeNumber()
        {
            var text = "@wait, -1"; // Negative seconds
            Assert.Throws<ChatCommandException>(() =>
            {
                var command = new WaitChatCommand(0, text.Split(','));
                Assert.Fail("Expected ChatCommandException was not thrown.");
            });
        }

        [Test]
        public void InvalidWaitCommand_ZeroSeconds()
        {
            var text = "@wait, 0"; // Zero seconds
            Assert.Throws<ChatCommandException>(() =>
            {
                var command = new WaitChatCommand(0, text.Split(','));
                Assert.Fail("Expected ChatCommandException was not thrown.");
            });
        }

        [Test]
        public void InvalidWaitCommand_TooLongWait()
        {
            var text = "@wait, 5.01"; // More than 5 seconds
            Assert.Throws<ChatCommandException>(() =>
            {
                var command = new WaitChatCommand(0, text.Split(','));
                Assert.Fail("Expected ChatCommandException was not thrown.");
            });
        }

        [Test]
        public void InvalidWaitCommand_TooManyArguments()
        {
            var text = "@wait, 2.5, Extra"; // Too many arguments
            Assert.Throws<ChatCommandException>(() =>
            {
                var command = new WaitChatCommand(0, text.Split(','));
                Assert.Fail("Expected ChatCommandException was not thrown.");
            });
        }
    }

    public class Goto
    {
        [Test]
        public void Parseable()
        {
            var text = "@goto, EndLabel";
            var command = new GotoChatCommand(0, text.Split(','));

            Assert.AreEqual(CommandType.Goto, command.Type);
            Assert.AreEqual("EndLabel", command.GotoLabel);
        }

        [Test]
        public void InvalidGotoCommand_MissingLabel()
        {
            var text = "@goto"; // Missing label argument
            Assert.Throws<ChatCommandException>(() =>
            {
                var command = new GotoChatCommand(0, text.Split(','));
                Assert.Fail("Expected ChatCommandException was not thrown.");
            });
        }

        [Test]
        public void InvalidGotoCommand_EmptyLabel()
        {
            var text = "@goto,   "; // Label is empty after trimming
            Assert.Throws<ChatCommandException>(() =>
            {
                var command = new GotoChatCommand(0, text.Split(','));
                Assert.Fail("Expected ChatCommandException was not thrown.");
            });
        }

        [Test]
        public void InvalidGotoCommand_TooManyArguments()
        {
            var text = "@goto, Label1, Label2"; // Too many arguments
            Assert.Throws<ChatCommandException>(() =>
            {
                var command = new GotoChatCommand(0, text.Split(','));
                Assert.Fail("Expected ChatCommandException was not thrown.");
            });
        }
    }
}
