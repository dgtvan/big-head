﻿namespace BigHead.UseCases.Projects;

public record ToDoItemDTO(int Id, string Title, string Description, bool IsComplete, int? ContributorId);
