drop table if exists [File];
drop table if exists Message;
drop table if exists Author;
drop table if exists Thread;


-- Create table: Author
CREATE TABLE Author (
    Id INT PRIMARY KEY IDENTITY,
    ReferenceId NVARCHAR(200),
    Name NVARCHAR(100) NOT NULL
);

-- Create table: Thread
CREATE TABLE Thread (
    Id INT PRIMARY KEY IDENTITY,
    ReferenceId NVARCHAR(200),
    Name NVARCHAR(200),
    Type NVARCHAR(50) NOT NULL,
    AiThreadId NVARCHAR(200),
    AiAssistantId NVARCHAR(200)
);

-- Create table: Message
CREATE TABLE Message (
    Id INT PRIMARY KEY IDENTITY,
    ReferenceId NVARCHAR(200),
    Text NVARCHAR(1000) NOT NULL,
    AiText NVARCHAR(1000) NULL,
    Timestamp DATETIME2 NOT NULL,
    AuthorId INT NOT NULL,
    ThreadId INT NOT NULL,
    FOREIGN KEY (AuthorId) REFERENCES Author(Id),
    FOREIGN KEY (ThreadId) REFERENCES Thread(Id)
);

-- Create table: File
CREATE TABLE [File] (
    Id INT PRIMARY KEY IDENTITY NOT NULL,
    ReferenceId NVARCHAR(200),
    FileName NVARCHAR(1000) NOT NULL,
    FileHashSha512 NVARCHAR(256) NOT NULL,
);