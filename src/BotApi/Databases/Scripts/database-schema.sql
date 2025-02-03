drop table if exists [AIMessage];
drop table if exists [AIAssistant];
drop table if exists [AIThread];

drop table if exists [File];
drop table if exists Message;
drop table if exists Author;
drop table if exists Thread;


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
    Text NVARCHAR(2048) NOT NULL,
    Timestamp DATETIME2 NOT NULL,
    AuthorId INT NOT NULL,
    ThreadId INT NOT NULL,
    FOREIGN KEY (AuthorId) REFERENCES Author(Id),
    FOREIGN KEY (ThreadId) REFERENCES Thread(Id)
);

-- Create table: File
CREATE TABLE [File] (
    Id INT PRIMARY KEY IDENTITY NOT NULL,
    ReferenceId NVARCHAR(200) NOT NULL,
    FileName NVARCHAR(1000) NOT NULL,
    FileHashSha512 NVARCHAR(256) NOT NULL,
);

-- Create table: AIThread
CREATE TABLE AIThread (
    Id INT PRIMARY KEY IDENTITY,
    ReferenceId NVARCHAR(200) NOT NULL,
    ThreadId INT NOT NULL,
    FOREIGN KEY (ThreadId) REFERENCES Thread(Id)
);

-- Create table: AIAssistant
CREATE TABLE AIAssistant (
    Id INT PRIMARY KEY IDENTITY,
    ReferenceId NVARCHAR(200) NOT NULL,
    AIThreadId INT NOT NULL,
    FOREIGN KEY (AIThreadId) REFERENCES AIThread(Id)
);

-- Create table: AIMessage
CREATE TABLE AIMessage (
    Id INT PRIMARY KEY IDENTITY,
    ReferenceId NVARCHAR(200) NOT NULL,
    Text NVARCHAR(2048) NOT NULL,
    MessageId INT NOT NULL,
    FOREIGN KEY (MessageId) REFERENCES Message(Id)
);

-- Create table: AIUserMessagePrompt
CREATE TABLE AIUserMessagePrompt (
    Id INT PRIMARY KEY IDENTITY,
    Prompt NVARCHAR(2048) NOT NULL,
);