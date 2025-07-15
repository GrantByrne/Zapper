# GitHub Issue Implementation Plan Generator

## Purpose
This prompt helps analyze a codebase and create detailed implementation plans for GitHub issues, then automatically updates the issue with the plan.

## Instructions for Claude

You are tasked with analyzing a codebase to create a comprehensive implementation plan for a GitHub issue. Follow these steps:

### Step 1: Understand the Issue
- Search for and read the GitHub issue details
- Understand what feature/fix is being requested
- Identify the type of work (enhancement, bug fix, integration, etc.)

### Step 2: Analyze the Codebase
Examine the following aspects:
1. **Project Structure**
   - How is the codebase organized?
   - What are the main projects/modules?
   - What patterns are used for similar features?

2. **Architecture Patterns**
   - What interfaces and base classes exist?
   - How are services registered and injected?
   - What is the API design pattern (REST, GraphQL, etc.)?

3. **Existing Integrations** (if applicable)
   - How are current integrations structured?
   - What common patterns do they follow?
   - What interfaces do they implement?

4. **Authentication & Security**
   - What authentication mechanisms exist?
   - How is authorization handled?
   - What security measures are in place?

5. **Data Layer**
   - What database is used?
   - How are entities structured?
   - What ORM/data access pattern is used?

### Step 3: Create Implementation Plan
Structure your plan with:

1. **Overview**
   - Brief summary of what will be implemented
   - High-level approach
   - Benefits to users

2. **Architecture Summary**
   - Where the new code will live
   - How it integrates with existing code
   - New components needed

3. **Implementation Phases**
   - Break down into logical phases
   - Each phase should be independently testable
   - Include specific tasks for each phase

4. **Technical Details**
   - New files/classes to create
   - API endpoints needed
   - Database changes required
   - External dependencies

5. **Security Considerations**
   - Authentication requirements
   - Data validation needs
   - Rate limiting or other protections

6. **Testing Strategy**
   - Unit test approach
   - Integration test needs
   - Manual testing steps

7. **Next Steps**
   - Clear starting point for implementation
   - Order of tasks to tackle
   - Any prerequisites or dependencies

### Step 4: Update GitHub Issue
- Use `gh issue comment` to add the plan to the issue
- Format with clear markdown sections
- Include code examples where helpful

## Template Usage

To use this prompt:
1. Specify the issue number: "Please analyze the codebase and create an implementation plan for GitHub issue #[NUMBER]"
2. Optionally provide context: "Focus on [SPECIFIC AREA] aspects"
3. Request no code changes: "Don't make any code changes, just create the plan"

## Example Request
```
I would like you to look over the codebase and come up with a plan for implementing GitHub issue #27. Once you've done that, you should update the GitHub issue. Please don't make any code changes for this.
```

## Key Points to Remember
- Use TodoWrite to track your analysis steps
- Search thoroughly before making assumptions
- Consider existing patterns and conventions
- Think about backward compatibility
- Consider performance implications
- Include security from the start
- Make the plan actionable with clear next steps

## Output Format
The plan should be posted as a GitHub comment with:
- Clear section headers
- Bullet points for lists
- Code blocks for examples
- Specific file paths and class names
- Dependencies and prerequisites clearly stated
- Practical next steps without arbitrary timelines