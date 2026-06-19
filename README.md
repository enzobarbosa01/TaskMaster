# TaskMaster

Sistema desktop de gerenciamento de tarefas com **priorização automática** e **elementos de gamificação**, desenvolvido em C# com Windows Forms e SQLite.

> Trabalho de Conclusão de Curso — Engenharia de Computação
> Fundação Hermínio Ometto (FHO)
> Autor: Enzo Barbosa das Neves
> Orientador: Prof. Dr. Maurício Acconcia Dias

---

## Sobre o projeto

O TaskMaster resolve dois problemas centrais das ferramentas de produtividade tradicionais: a ausência de **priorização inteligente** das tarefas e a falta de **engajamento sustentado** do usuário ao longo do tempo.

O sistema calcula automaticamente um *score* de prioridade para cada tarefa, combinando importância e urgência, e aplica mecânicas de gamificação — XP, níveis, conquistas e streak — para manter o usuário engajado no uso diário.

## Funcionalidades

- CRUD completo de tarefas (criar, editar, excluir, concluir)
- Priorização automática por algoritmo de score
- Categorias e subtarefas (checklist)
- Tarefas recorrentes (diária, semanal, mensal)
- Filtros por categoria, status e busca textual
- Alertas visuais de prazo próximo ou vencido
- Sistema de XP e progressão de níveis
- 10 conquistas desbloqueáveis
- Streak de dias produtivos consecutivos
- Dashboard com métricas e gráficos de evolução
- Histórico de atividades

## Algoritmo de priorização

O diferencial técnico do sistema é a fórmula de cálculo de prioridade:

```
Score = (Importância x 0,6) + (Urgência x 0,4)
Urgência = 1 / (dias_restantes + 1)
```

A importância vem do nível de prioridade definido pelo usuário (Baixa = 0,25 até Crítica = 1,00). A urgência cresce conforme o prazo se aproxima, chegando ao máximo em tarefas vencidas. A ponderação 60/40 favorece tarefas importantes sobre tarefas apenas urgentes, evitando que prazos de baixo impacto dominem a lista.

## Tecnologias utilizadas

| Tecnologia | Uso |
|---|---|
| C# / .NET 6 | Linguagem e framework principal |
| Windows Forms | Interface gráfica |
| SQLite (Microsoft.Data.Sqlite) | Persistência de dados |
| Arquitetura MVC | Organização em camadas (Model, View, Controller) |
| Padrão Repository | Abstração de acesso a dados |

## Arquitetura

```
TaskMaster/
├── Models/          -> Entidades de domínio (Tarefa, Categoria, Usuario, Conquista...)
├── Data/             -> DatabaseManager e Repositories (acesso ao SQLite)
├── Controllers/      -> Lógica de negócio (TarefaController, GamificacaoController)
├── Views/
│   ├── Forms/        -> Telas principais (FormMain, FormTarefa, Dashboard...)
│   └── Controls/     -> Componentes de UI customizados
└── Utils/            -> Tema visual (cores e fontes)
```

## Como executar

### Pré-requisitos
- Visual Studio 2022
- Workload **.NET Desktop Development** instalado

### Passos

1. Clone o repositório:
   ```bash
   git clone https://github.com/SEU-USUARIO/TaskMaster.git
   ```
2. Abra o arquivo `TaskMaster.csproj` no Visual Studio
3. Aguarde o restore automático dos pacotes NuGet
4. Pressione `F5` para compilar e executar

O banco de dados SQLite é criado automaticamente em `%AppData%\TaskMaster\taskmaster.db` na primeira execução, já populado com categorias padrão e as 10 conquistas do sistema.

## Testes e validação

- **Testes funcionais**: 13 casos de teste documentados, cobrindo os 13 requisitos funcionais identificados — todos aprovados
- **Avaliação de usabilidade**: questionário SUS (System Usability Scale) aplicado a 5 participantes, com score médio de **83,5 pontos**, classificado como "Bom / Excelente" segundo Bangor, Kortum e Miller (2008)

## Trabalhos futuros

- [ ] Exportação de relatórios em PDF
- [ ] Migração para .NET MAUI ou Avalonia (suporte multiplataforma)
- [ ] Modo colaborativo com sincronização via API REST
- [ ] Algoritmo de recomendação de horários baseado no histórico do usuário

## Referências principais

- ALLEN, David. *Getting Things Done*. Viking Penguin, 2001.
- CHOU, Yu-kai. *Actionable Gamification*. Octalysis Media, 2015.
- WERBACH, Kevin; HUNTER, Dan. *For the Win*. Wharton Digital Press, 2012.
- PRESSMAN, Roger S.; MAXIM, Bruce R. *Engenharia de Software*. 8. ed. AMGH, 2016.
- SOMMERVILLE, Ian. *Engenharia de Software*. 9. ed. Pearson, 2011.

A lista completa de referências está disponível no documento do TCC.

## Licença

Projeto acadêmico desenvolvido para fins de Trabalho de Conclusão de Curso.
