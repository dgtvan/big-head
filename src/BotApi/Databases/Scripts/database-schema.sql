drop table if exists [OpenAiMessage];
drop table if exists [OpenAiThread];
drop table if exists [OpenAiAssistant];

drop table if exists [File];
drop table if exists Message;
drop table if exists Author;
drop table if exists Thread;
drop table if exists [PromptTemplate];


-- Create table: Author
CREATE TABLE Author (
    Id INT PRIMARY KEY IDENTITY,
    ReferenceId NVARCHAR(200) NOT NULL,
    Name NVARCHAR(100) NOT NULL
);

-- Create table: Thread
CREATE TABLE Thread (
    Id INT PRIMARY KEY IDENTITY,
    ReferenceId NVARCHAR(200) NOT NULL,
    Name NVARCHAR(200),
    Type NVARCHAR(50) NOT NULL,
);

-- Create table: Message
CREATE TABLE Message (
    Id INT PRIMARY KEY IDENTITY,
    ReferenceId NVARCHAR(200) NOT NULL,
    Text NVARCHAR(MAX) NOT NULL,
    Timestamp DATETIME2 NOT NULL,
    AuthorId INT NOT NULL,
    ThreadId INT NOT NULL,
    FOREIGN KEY (AuthorId) REFERENCES Author(Id),
    FOREIGN KEY (ThreadId) REFERENCES Thread(Id),
);

-- Create table: File
CREATE TABLE [File] (
    Id INT PRIMARY KEY IDENTITY NOT NULL,
    ReferenceId NVARCHAR(200) NOT NULL,
    FileName NVARCHAR(1000) NOT NULL,
    FileHashSha512 NVARCHAR(256) NOT NULL,
);

-- Create table: PromptTemplate
CREATE TABLE PromptTemplate(
    Id INT PRIMARY KEY IDENTITY,
    Type NVARCHAR(100) NOT NULL,
    Prompt NVARCHAR(2048) NOT NULL
);

-- Create table: OpenAiAssistant
CREATE TABLE OpenAiAssistant (
    ThreadId INT NOT NULL,
    OpenAiThreadId NVARCHAR(200) NOT NULL,
);

-- Create table: OpenAiThread
CREATE TABLE OpenAiThread (
    ThreadId INT NOT NULL,
    OpenAiThreadId NVARCHAR(200) NOT NULL,
    FOREIGN KEY (ThreadId) REFERENCES Thread(Id),
);

-- Create table: OpenAiMessage
CREATE TABLE OpenAiMessage (
    MessageId INT NOT NULL,
    OpenAiMessageId NVARCHAR(200) NULL,
    Text NVARCHAR(MAX) NOT NULL,
    FOREIGN KEY (MessageId) REFERENCES Message(Id)
);