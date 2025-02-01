drop table if exists [File];
drop table if exists Message;
drop table if exists Author;
drop table if exists Thread;


-- Create Author Table
CREATE TABLE Author (
    Id INT PRIMARY KEY IDENTITY,
    ReferenceId NVARCHAR(200),
    Name NVARCHAR(100) NOT NULL
);

-- Create Thread Table
CREATE TABLE Thread (
    Id INT PRIMARY KEY IDENTITY,
    ReferenceId NVARCHAR(200),
    Name NVARCHAR(200),
    Type NVARCHAR(50) NOT NULL,
    AiThreadId NVARCHAR(200),
    AiAssistantId NVARCHAR(200)
);

-- Create Message Table
CREATE TABLE Message (
    Id INT PRIMARY KEY IDENTITY,
    ReferenceId NVARCHAR(200),
    Text NVARCHAR(1000) NOT NULL,
    Timestamp DATETIME2 NOT NULL,
    AuthorId INT NOT NULL,
    ThreadId INT NOT NULL,
    FOREIGN KEY (AuthorId) REFERENCES Author(Id),
    FOREIGN KEY (ThreadId) REFERENCES Thread(Id)
);

-- Create File Table
CREATE TABLE [File] (
    Id INT PRIMARY KEY IDENTITY NOT NULL,
    ReferenceId NVARCHAR(200),
    FileName NVARCHAR(1000) NOT NULL,
    FileHashSha512 NVARCHAR(256) NOT NULL,
);