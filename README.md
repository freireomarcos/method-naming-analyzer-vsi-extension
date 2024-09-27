# Extensão Visual Studio - Analyzers e Code Fixes

Este projeto é uma extensão para o Visual Studio que adiciona analisadores de código (analyzers) e correções automáticas (code fixes) para melhorar a qualidade e a legibilidade do código C#.

## Analyzers e Code Fixes Disponíveis

### 1. **AsyncMethodNameAnalyzer**
   - **Regra**: Todo método assíncrono deve terminar com o sufixo `Async`. (A menos que o metodo seja no `Controller`)
   - **Motivo**: Seguir as convenções de nomenclatura do C# torna o código mais previsível e fácil de entender.
   - **Correção**: Adiciona automaticamente o sufixo `Async` a métodos assíncronos que não o possuem.

### 2. **IfBracesAnalyzer**
   - **Regra**: Todo bloco de código `if` deve estar envolto em chaves `{}`.
   - **Motivo**: O uso de chaves, mesmo em blocos `if` de uma única linha, aumenta a clareza e previne erros ao adicionar novas linhas de código.
   - **Correção**: Adiciona automaticamente as chaves aos blocos `if` que não as possuem.

### 3. **ReturnWithoutVariableAnalyzer**
   - **Regra**: O valor retornado por um método deve ser atribuído a uma variável antes de ser retornado.
   - **Motivo**: Melhorar a legibilidade e facilitar o debug, tornando mais fácil entender qual valor está sendo retornado.
   - **Correção**: Adiciona automaticamente uma variável (`response`) para armazenar o valor de retorno antes da instrução `return`.

## Instalação

Para utilizar a extensão, basta clonar este repositório e seguir as instruções abaixo:

1. Abra o projeto no Visual Studio.
2. Compile o projeto.
3. Instale a extensão no Visual Studio (ou faça o deploy via Marketplace se estiver disponível).

## Licença

Este projeto está licenciado sob a [MIT License](LICENSE).
