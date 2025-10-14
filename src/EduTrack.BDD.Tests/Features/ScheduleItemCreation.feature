Feature: Schedule Item Creation
    As a teacher
    I want to create schedule items for my teaching plan
    So that I can assign tasks and activities to my students

    Background:
        Given I am logged in as a teacher
        And I have a teaching plan "Advanced Mathematics"

    @smoke
    Scenario: Create a simple writing assignment
        Given I am on the schedule item creation page
        When I fill in the basic information:
            | Field | Value |
            | Type | Writing |
            | Title | Homework Assignment |
            | Description | Complete exercises 1-10 |
        And I set the schedule:
            | Field | Value |
            | Start Date | 1403/01/01 |
            | Due Date | 1403/01/08 |
            | Start Time | 09:00 |
            | Due Time | 17:00 |
        And I set it as mandatory with max score 100
        And I assign it to students
        And I add content with instructions "Please complete all exercises"
        When I save the schedule item
        Then the schedule item should be created successfully

    @regression
    Scenario: Create a quiz with multiple choice questions
        Given I am on the schedule item creation page
        When I fill in the basic information:
            | Field | Value |
            | Type | Quiz |
            | Title | Chapter 1 Quiz |
            | Description | Test your knowledge of basic concepts |
        And I set the schedule:
            | Field | Value |
            | Start Date | 1403/01/15 |
            | Due Date | 1403/01/22 |
            | Start Time | 10:00 |
            | Due Time | 11:00 |
        And I set it as mandatory with max score 50
        And I assign it to students
        And I add quiz content with 10 multiple choice questions
        When I save the schedule item
        Then the schedule item should be created successfully

    @error-handling
    Scenario: Attempt to create assignment with invalid data
        Given I am on the schedule item creation page
        When I fill in the basic information:
            | Field | Value |
            | Type | Writing |
            | Title | "" |
            | Description | Test Description |
        And I set the schedule:
            | Field | Value |
            | Start Date | 1403/01/01 |
            | Due Date | 1403/01/08 |
        When I try to save the schedule item
        Then I should see an error message "Title cannot be empty"
        And the schedule item should not be created
