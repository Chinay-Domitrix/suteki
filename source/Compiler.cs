using System;
using System.Collections.Generic;

namespace Suteki
{
    class Compiler
    {
        public Dictionary<string, Module>     Modules;
        public Dictionary<string, Symbol>     Symbols;
        public Dictionary<string, Symbol>     Locals;
        public int                            MainFile;
        public Scanner                        CurrentScanner;
        public bool                           HadError;
        public Dictionary<string, UserType>   UserTypes;
        public FileInput                      CurrentInput;
        public uint                           TabCount;

        // Initialize the Compiler
        public Compiler()
        {
            Modules   = new Dictionary<string, Module>();
            Symbols   = new Dictionary<string, Symbol>();
            Locals    = new Dictionary<string, Symbol>();
            UserTypes = new Dictionary<string, UserType>();
            MainFile  = -1;
            HadError  = false;
            TabCount  = 0;
        }

        // Get previous Token
        public Token PreviousToken
        {
            get 
            { 
                return CurrentScanner.PreviousToken; 
            }
        }

        // Get current Token
        public Token CurrentToken
        {
            get 
            { 
                return CurrentScanner.CurrentToken; 
            }
        }

        // Show error
        public void Error(string message)
        {
            HadError = true;

            Utilities.WriteColor(ConsoleColor.Red,   "Error: ");
            Utilities.WriteColor(ConsoleColor.White, message);
            Console.WriteLine();
        }

        // Show error at Token
        public void Error(Token token, string message)
        {
            HadError = true;
            Utilities.WriteColor(ConsoleColor.Red, $"[{CurrentInput.Path}:{token.Line}:{token.Column}] Error");

            if (token.Type == TokenType.End)
                Utilities.WriteColor(ConsoleColor.Red, " at end: ");
            else if (token.Type == TokenType.Error)
                Utilities.WriteColor(ConsoleColor.Red, ": ");
            else
                Utilities.WriteColor(ConsoleColor.Red, " at ", ConsoleColor.White, token.Data, ConsoleColor.Red, ": ");

            Utilities.WriteColor(ConsoleColor.White, message);
            Console.WriteLine();
        }

        // Show warning
        public void Warning(string message)
        {
            Utilities.WriteColor(ConsoleColor.Yellow, "Warning: ");
            Utilities.WriteColor(ConsoleColor.White,  message);
            Console.WriteLine();
        }

        // Show warning at Token
        public void Warning(Token token, string message)
        {
            Utilities.WriteColor(ConsoleColor.Yellow, $"[{CurrentInput.Path}:{token.Line}:{token.Column}] Warning");

            if (token.Type == TokenType.End)
                Utilities.WriteColor(ConsoleColor.Yellow, " at end: ");
            else if (token.Type == TokenType.Error)
                Utilities.WriteColor(ConsoleColor.Yellow, ": ");
            else
                Utilities.WriteColor(ConsoleColor.Yellow, " at ", ConsoleColor.White, token.Data, ConsoleColor.Red, ": ");

            Utilities.WriteColor(ConsoleColor.White, message);
            Console.WriteLine();
        }

        // Advance the Token
        public void Advance()
        {
            for (;;)
            {
                CurrentScanner.Scan();

                if (CurrentToken.Type != TokenType.Error)
                    break;

                Error(CurrentToken, (string)CurrentToken.Data);
            }
        }

        // Match Token?
        public bool Match(TokenType type)
        {
            if (CurrentToken.Type == type)
            {
                Advance();
                return true;
            }

            return false;
        }

        // Consume Token
        public bool Consume(TokenType type, string message)
        {
            if (Match(type))
                return true;

            Error(CurrentToken, message);
            return false;
        }

        // Is token a type?
        public bool IsType(Token token)
        {
            if (token.Type >= TokenType.Void && token.Type <= TokenType.Double)
                return true;
            else
                return false;
        }

        // Get type as C type
        public string getTypeAsCType(Token typeToken)
        {
            switch (typeToken.Type)
            {
                case TokenType.Void:
                    return "void";

                case TokenType.Bool:
                    return "_Bool";
                    
                case TokenType.UByte:
                    return "unsigned char";
                    
                case TokenType.UShort:
                    return "unsigned short";
                    
                case TokenType.UInt:
                    return "unsigned int";
                    
                case TokenType.ULong:
                    return "unsigned long long";
                    
                case TokenType.Byte:
                    return "char";
                    
                case TokenType.Short:
                    return "short";
                    
                case TokenType.Int:
                    return "int";
                    
                case TokenType.Long:
                    return "long long";
                    
                case TokenType.Single:
                    return "float";
                    
                case TokenType.Double:
                    return "double";
            }

            return "void";
        }

        // Get type name
        public string getTypeName(Token typeToken)
        {
            switch (typeToken.Type)
            {
                case TokenType.Void:
                    return "vd";

                case TokenType.Bool:
                    return "bl";
                    
                case TokenType.UByte:
                    return "ube";
                    
                case TokenType.UShort:
                    return "ust";
                    
                case TokenType.UInt:
                    return "uit";
                    
                case TokenType.ULong:
                    return "ulg";
                    
                case TokenType.Byte:
                    return "be";
                    
                case TokenType.Short:
                    return "st";
                    
                case TokenType.Int:
                    return "it";
                    
                case TokenType.Long:
                    return "lg";
                    
                case TokenType.Single:
                    return "se";
                    
                case TokenType.Double:
                    return "de";
            }

            return "vd";
        }

        // Learn everything
        public bool Learn()
        {
            Dictionary<string, Symbol> symbols = new Dictionary<string, Symbol>();
            Dictionary<string, Symbol> symbolTable;

            // Loop thorugh every file and learn every global 
            // symbol from it
            for (int index = 0; index < Program.Inputs.Count; ++index)
            {
                symbols.Clear();

                CurrentInput   = Program.Inputs[index];
                CurrentScanner = new Scanner(CurrentInput.Source);

                // Start learning everything possible
                Advance();

                while (!Match(TokenType.End))
                {
                    Advance();

                    switch (PreviousToken.Type)
                    {
                        // Export module?
                        case TokenType.Export:
                        {
                            // Parse the module name
                            Token  startName  = CurrentToken;
                            string moduleName = "";

                            for (;;)
                            {
                                if (Match(TokenType.Identifier))
                                    moduleName += (string)PreviousToken.Data;
                                else
                                {
                                    startName.Data = moduleName;
                                    Error(startName, "Invalid module name.");
                                }

                                if (!Match(TokenType.Dot))
                                    break;

                                moduleName += '.';
                            }

                            Consume(TokenType.Semicolon, "Expected ';' after module name.");

                            // Make the module
                            CurrentInput.ModuleName = moduleName;

                            if (!Modules.ContainsKey(moduleName))
                            {
                                Modules[moduleName]           = new Module();
                                Modules[moduleName].FileInput = index;
                            }
                            else
                            {
                                startName.Data = moduleName;
                                Error(startName, "This module was already declared.");
                            }

                            break;
                        }

                        // Declaration?
                        default:
                        {
                            if (!IsType(PreviousToken))
                                break;

                            Token  nameToken;
                            string nameString;

                            // Parse declaration name
                            Consume(TokenType.Identifier, "Expected identifier after type.");

                            nameToken  = PreviousToken;
                            nameString = (string)PreviousToken.Data + '_';

                            // Make symbol
                            Symbol symbol = new Symbol();

                            if (Match(TokenType.LeftParenthesis))
                            {
                                symbol.Type = SymbolType.Function;

                                if (!Match(TokenType.RightParenthesis))
                                {
                                    do
                                    {
                                        Advance();

                                        if (!IsType(PreviousToken))
                                        {
                                            Error(PreviousToken, "Expected type.");
                                            break;
                                        }

                                        nameString += getTypeName(PreviousToken);
                                        Consume(TokenType.Identifier, "Expected parameter name after type.");
                                    }
                                    while (Match(TokenType.Comma));
                                    
                                    Consume(TokenType.RightParenthesis, "Expected ')' after function parameters.");
                                }
                                else
                                    nameString = nameString.Substring(0, nameString.Length - 1);
                            }
                            else
                            {
                                symbol.Type = SymbolType.Variable;
                                nameString  = nameString.Substring(0, nameString.Length - 1);
                            }

                            symbol.Name = nameString;

                            // Make sure that symbol was not declared before
                            if (symbols.ContainsKey(nameString))
                                Error(nameToken, "This symbol was already declared.");
                            
                            // Is entry point function declaration?
                            if ((string)nameToken.Data == "main" && symbol.Type == SymbolType.Function)
                            {
                                if (MainFile != -1)
                                    Error(nameToken, "The main function was already declared.");
        
                                MainFile = index;
                            }

                            // Add symbol
                            symbols[nameString] = symbol;

                            // Skip other tokens that are not going to be used
                            if (symbol.Type == SymbolType.Variable)
                            {
                                if (!Match(TokenType.Semicolon))
                                {
                                    while (!Match(TokenType.Semicolon) && !Match(TokenType.End))
                                        Advance();

                                    if (PreviousToken.Type == TokenType.End)
                                        Error(PreviousToken, "Expected ';' after variable declaration.");
                                }
                            }
                            else
                            {
                                Consume(TokenType.LeftBrace, "Expected '{' after ')'.");

                                if (!Match(TokenType.RightBrace))
                                {
                                    while (!Match(TokenType.RightBrace) && !Match(TokenType.End))
                                        Advance();

                                    if (PreviousToken.Type == TokenType.End)
                                        Error(PreviousToken, "Expected '}' after function statement(s).");
                                }
                            }
                            break;
                        }
                    }
                }

                // Add all symbols to module symbols or global symbols
                if (CurrentInput.ModuleName != "")
                    symbolTable = Modules[CurrentInput.ModuleName].Symbols;
                else
                    symbolTable = Symbols;

                foreach (KeyValuePair<string, Symbol> entry in symbols)
                    symbolTable[entry.Key] = entry.Value;
            }

            // Make sure we found entry point file
            if (MainFile == -1)
            {
                Error("Could not find entry point file.");
                return false;
            }

            return !HadError;
        }

        // Generate function declaration
        public void GenerateFunctionDeclaration(Token typeToken, Token nameToken)
        {
            // Parse the function header
            string functionHeader     = "";
            string functionParameters = "(";

            functionHeader += getTypeAsCType(typeToken);
            functionHeader += $" g_su_{(string)nameToken.Data}_";

            if (!Match(TokenType.RightParenthesis))
            {
                do
                {
                    Advance();
                    functionHeader     += getTypeName(PreviousToken);
                    functionParameters += getTypeAsCType(PreviousToken) + ' ';

                    Advance();
                    functionParameters += (string)PreviousToken.Data + ", ";
                }
                while (Match(TokenType.Comma));
                
                Advance();
                functionParameters = functionParameters.Substring(0, functionParameters.Length - 2);
            }
            else
                functionHeader = functionHeader.Substring(0, functionHeader.Length - 1);

            functionParameters += ')';
            functionHeader     += functionParameters;

            // Add to header file
            CurrentInput.HeaderOutput += $"extern {functionHeader};\n";

            // Add to source file
            CurrentInput.SourceOutput += $"{functionHeader}\n";

            // Parse function body
            Advance();
            CurrentInput.SourceOutput += "{\n";
            ++TabCount;

            while (!Match(TokenType.RightBrace))
                GenerateStatement();

            --TabCount;
            CurrentInput.SourceOutput += "}\n\n";
        }

        // Generate variable declaration
        public void GenerateVariableDeclaration(Token typeToken, Token nameToken, bool isGlobal)
        {
            string nameString = (string)nameToken.Data;

            if (isGlobal)
            {
                CurrentInput.HeaderOutput +=  "extern ";
                CurrentInput.HeaderOutput +=  getTypeAsCType(typeToken);
                CurrentInput.HeaderOutput += $" g_su_{(string)nameToken.Data};\n";
            }
            else
            {
                if (Symbols.ContainsKey(nameString) || Locals.ContainsKey(nameString))
                    Error(nameToken, "This symbol was already declared.");

                Locals[nameString] = new Symbol(SymbolType.Variable, nameString);
            }

            CurrentInput.SourceOutput += getTypeAsCType(typeToken);

            if (isGlobal)
                CurrentInput.SourceOutput += " g_";
            else
                CurrentInput.SourceOutput += ' ';

            CurrentInput.SourceOutput += $"su_{(string)nameToken.Data}";

            if (!Match(TokenType.Semicolon))
            {
                if (Match(TokenType.Equal))
                {
                    CurrentInput.SourceOutput += " = ";
                    GenerateExpression();
                }
                else
                    Error(CurrentToken, "Expected '=' or ';' after variable name.");

                Consume(TokenType.Semicolon, "Expected ';' after variable declaration.");
            }

            CurrentInput.SourceOutput += ";\n";
        }

        // Generate variable assignment
        public void GenerateVariableAssignment(Token nameToken)
        {
            string nameString = (string)nameToken.Data;

            if (Symbols.ContainsKey(nameString))
            {
                CurrentInput.SourceOutput += "g_";

                Symbol symbol = Symbols[nameString];

                if (symbol.Type != SymbolType.Variable)
                    Error(nameToken, "This variable does not exists.");
            }
            else if (Locals.ContainsKey(nameString))
            {
                Symbol symbol = Locals[nameString];

                if (symbol.Type != SymbolType.Variable)
                    Error(nameToken, "This variable does not exists.");
            }
            else
                Error(nameToken, "This variable does not exists.");

            CurrentInput.SourceOutput += $"su_{nameString}";
            CurrentInput.SourceOutput +=  " = ";
            GenerateExpression();
            
            Consume(TokenType.Semicolon, "Expected ';' after variable assignment.");
            CurrentInput.SourceOutput += ";\n";
        }

        // Generate expression
        public void GenerateExpression()
        {
            if (Match(TokenType.Number))
                CurrentInput.SourceOutput += PreviousToken.Data.ToString();
            else
                Error(CurrentToken, "Unexpected token.");
        }

        // Generate return statement
        public void GenerateReturnStatement()
        {
            CurrentInput.SourceOutput += "return";

            // Parse value
            if (Match(TokenType.Semicolon))
                CurrentInput.SourceOutput += ";\n";
            else
            {
                CurrentInput.SourceOutput += ' ';
                GenerateExpression();
                CurrentInput.SourceOutput += ";\n";

                Consume(TokenType.Semicolon, "Expected ';' after return expression.");
            }
        }

        // Generate statement
        public void GenerateStatement()
        {
            Advance();

            // Generate tabs
            for (uint i = 0; i < TabCount; ++i)
                CurrentInput.SourceOutput += '\t';

            switch (PreviousToken.Type)
            {
                case TokenType.Return:
                {
                    GenerateReturnStatement();
                    break;
                }

                default:
                {
                    if (!IsType(PreviousToken))
                    {
                        Token previousToken = PreviousToken;

                        if (Match(TokenType.Equal))
                            GenerateVariableAssignment(previousToken);
                        else
                        {
                            // Remove tabs
                            CurrentInput.SourceOutput = CurrentInput.SourceOutput.Substring(0, 
                                    (int)(CurrentInput.SourceOutput.Length - TabCount));

                            Error(PreviousToken, "Unexpected token.");
                        }
                        break;
                    }

                    Token typeToken = PreviousToken;
                    Token nameToken;

                    // Parse declaration name
                    Consume(TokenType.Identifier, "Expected identifier after type.");
                    nameToken  = PreviousToken;

                    // Generate declaration
                    GenerateVariableDeclaration(typeToken, nameToken, false);
                    break;
                }
            }
        }

        // Start compiling 
        public bool Start()
        {
            if (!Learn())
                return false;

            // Loop thorugh every file and generate C files
            for (int index = 0; index < Program.Inputs.Count; ++index)
            {
                CurrentInput   = Program.Inputs[index];
                CurrentScanner = new Scanner(CurrentInput.Source);

                // Start learning everything possible
                Advance();

                while (!Match(TokenType.End))
                {
                    Advance();

                    switch (PreviousToken.Type)
                    {
                        // Export module?
                        case TokenType.Export:
                        {
                            // Skip module name
                            for (;;)
                            {
                                Match(TokenType.Identifier);

                                if (!Match(TokenType.Dot))
                                    break;
                            }

                            Advance();
                            break;
                        }

                        // Import module
                        case TokenType.Import:
                        {
                            // Parse the module name
                            Token  startName  = CurrentToken;
                            string moduleName = "";

                            for (;;)
                            {
                                if (Match(TokenType.Identifier))
                                    moduleName += (string)PreviousToken.Data;
                                else
                                {
                                    startName.Data = moduleName;
                                    Error(startName, "Invalid module name.");
                                }

                                if (!Match(TokenType.Dot))
                                    break;

                                moduleName += '.';
                            }

                            Consume(TokenType.Semicolon, "Expected ';' after module name.");

                            // Make sure that module exists
                            if (!Modules.ContainsKey(moduleName))
                            {
                                startName.Data = moduleName;
                                Error(startName, "Could not find module.");
                                break;
                            }

                            // Make sure that module wasn't imported already
                            if (CurrentInput.Imports.Contains(moduleName))
                            {
                                startName.Data = moduleName;
                                Warning(startName, "This module was already imported.");
                                break;
                            }

                            // Add import
                            CurrentInput.Imports.Add(moduleName);

                            // Add module symbols to global symbols
                            foreach (KeyValuePair<string, Symbol> entry in Modules[moduleName].Symbols)
                                Symbols[entry.Key] = entry.Value;

                            break;
                        }

                        // Declaration?
                        default:
                        {
                            if (!IsType(PreviousToken))
                            {
                                Error(PreviousToken, "Unexpected token.");
                                break;
                            }

                            Token typeToken = PreviousToken;
                            Token nameToken;

                            // Parse declaration name
                            Consume(TokenType.Identifier, "Expected identifier after type.");
                            nameToken  = PreviousToken;

                            // Generate declaration
                            if (Match(TokenType.LeftParenthesis))
                                GenerateFunctionDeclaration(typeToken, nameToken);
                            else
                                GenerateVariableDeclaration(typeToken, nameToken, true);

                            break;
                        }
                    }
                }
            }

            return !HadError;
        }
    }
}