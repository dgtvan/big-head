drop table if exists [File];
drop table if exists Message;
drop table if exists Author;
drop table if exists Thread;

drop table if exists [AIMessage];
drop table if exists [AIThread];
drop table if exists [AIAssistant];
drop table if exists [AiPrompt];

-- Create table: AiAssistant
CREATE TABLE AiAssistant (
    Id INT PRIMARY KEY IDENTITY,
    ReferenceId NVARCHAR(200) NOT NULL
);

-- Create table: AiThread
CREATE TABLE AiThread (
    Id INT PRIMARY KEY IDENTITY,
    ReferenceId NVARCHAR(200) NOT NULL,
    AiAsisstantId INT NULL,
    FOREIGN KEY (AiAsisstantId) REFERENCES AiAssistant(Id)
);

-- Create table: AiMessage
CREATE TABLE AiMessage (
    Id INT PRIMARY KEY IDENTITY,
    ReferenceId NVARCHAR(200) NOT NULL,
    Text NVARCHAR(2048) NOT NULL,
);

-- Create table: AiPrompt
CREATE TABLE AiPrompt (
    Id INT PRIMARY KEY IDENTITY,
    Type NVARCHAR(100) NOT NULL,
    Prompt NVARCHAR(2048) NOT NULL,
);


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
    AiThreadId INT NULL,
    FOREIGN KEY (AiThreadId) REFERENCES AIThread(Id)
);

-- Create table: Message
CREATE TABLE Message (
    Id INT PRIMARY KEY IDENTITY,
    ReferenceId NVARCHAR(200) NOT NULL,
    Text NVARCHAR(2048) NOT NULL,
    Timestamp DATETIME2 NOT NULL,
    AuthorId INT NOT NULL,
    ThreadId INT NOT NULL,
    AiMessageId INT NULL,
    FOREIGN KEY (AuthorId) REFERENCES Author(Id),
    FOREIGN KEY (ThreadId) REFERENCES Thread(Id),
    FOREIGN KEY (AiMessageId) REFERENCES AiMessage(Id)
);

-- Create table: File
CREATE TABLE [File] (
    Id INT PRIMARY KEY IDENTITY NOT NULL,
    ReferenceId NVARCHAR(200) NOT NULL,
    FileName NVARCHAR(1000) NOT NULL,
    FileHashSha512 NVARCHAR(256) NOT NULL,
);