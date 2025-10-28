# Requirements Document

## Introduction

Together is a WPF-based desktop application designed to strengthen emotional connections between partners and close friends. The system combines private couple features (shared journals, mood tracking, relationship goals) with social community features (posts, user walls, follow systems). The application follows Clean Architecture principles, MVVM pattern, and SOLID design principles to ensure maintainability and scalability.

## Glossary

- **Together System**: The complete WPF desktop application including all modules and features
- **User**: An individual registered account holder who can use both couple and social features
- **Partner**: A User who has established a couple connection with another User
- **Couple Connection**: A mutual relationship link between two Users enabling private shared features
- **Post**: User-generated content (text, image, or quote) visible on the social feed
- **Mood Entry**: A timestamped emotional state record with optional notes
- **Shared Journal**: A private collaborative space where Partners can write entries visible to each other
- **Together Moment**: A feed item showing partner activity, mood, or milestone
- **Love Streak**: A counter tracking consecutive days of interaction between Partners
- **Challenge**: A time-bound task or activity designed to encourage couple engagement
- **Virtual Pet**: A digital companion that evolves based on couple interaction frequency
- **User Wall**: A User's personal profile page displaying their posts and information
- **Follow Relationship**: A one-way connection allowing a User to see another User's public posts
- **Supabase**: The cloud database service used for data persistence and synchronization
- **Real-time Service**: The communication infrastructure enabling instant updates across clients

## Requirements

### Requirement 1: User Authentication and Account Management

**User Story:** As a new user, I want to create an account and log in securely, so that I can access the application and protect my personal data.

#### Acceptance Criteria

1. WHEN a User provides valid registration credentials (email, username, password), THE Together System SHALL create a new User account with encrypted password storage
2. WHEN a User provides valid login credentials, THE Together System SHALL authenticate the User and grant access to the application
3. IF a User provides invalid login credentials, THEN THE Together System SHALL display an error message and deny access
4. THE Together System SHALL enforce password requirements of minimum 8 characters with at least one uppercase letter, one lowercase letter, and one number
5. WHEN a User requests password reset, THE Together System SHALL send a verification email with a time-limited reset link

### Requirement 2: Couple Connection Establishment

**User Story:** As a user, I want to connect with my partner within the app, so that we can access shared private features together.

#### Acceptance Criteria

1. WHEN a User sends a couple connection request to another User, THE Together System SHALL create a pending connection request
2. WHEN a User receives a couple connection request, THE Together System SHALL notify the User and display the request for approval
3. WHEN a User accepts a couple connection request, THE Together System SHALL establish a mutual Couple Connection between both Users
4. THE Together System SHALL limit each User to one active Couple Connection at any given time
5. WHEN a User terminates a Couple Connection, THE Together System SHALL archive shared data and remove access to couple-specific features

### Requirement 3: Shared Journal Management

**User Story:** As a partner, I want to write journal entries that my partner can read, so that we can share our thoughts and experiences privately.

#### Acceptance Criteria

1. WHEN a Partner creates a journal entry with text content, THE Together System SHALL save the entry with timestamp and author information
2. WHILE a Couple Connection is active, THE Together System SHALL display all Shared Journal entries from both Partners in chronological order
3. WHEN a Partner views the Shared Journal, THE Together System SHALL mark entries as read and update the read status
4. THE Together System SHALL allow Partners to attach images to journal entries with maximum size of 5MB per image
5. WHEN a Partner deletes their own journal entry, THE Together System SHALL remove the entry from both Partners' views

### Requirement 4: Mood Tracking and Emotional Support

**User Story:** As a user, I want to log my current mood, so that my partner can understand my emotional state and offer support.

#### Acceptance Criteria

1. WHEN a User selects a mood indicator (emoji or text), THE Together System SHALL create a Mood Entry with timestamp
2. WHEN a Partner logs a negative mood (sad, anxious, angry), THE Together System SHALL notify the other Partner with a supportive message suggestion
3. THE Together System SHALL display mood history for the past 30 days in a visual chart format
4. WHILE viewing mood history, THE Together System SHALL calculate and display mood patterns and trends
5. WHEN a User adds notes to a Mood Entry, THE Together System SHALL store the notes and make them visible only to connected Partners

### Requirement 5: Shared To-Do List and Planning

**User Story:** As a partner, I want to create and manage shared tasks with my partner, so that we can coordinate our activities and responsibilities.

#### Acceptance Criteria

1. WHEN a Partner creates a to-do item with title and optional due date, THE Together System SHALL add the item to the shared list visible to both Partners
2. WHEN a Partner marks a to-do item as complete, THE Together System SHALL update the status and notify the other Partner
3. THE Together System SHALL allow Partners to assign to-do items to either partner or both
4. WHEN a to-do item due date passes without completion, THE Together System SHALL highlight the item as overdue
5. THE Together System SHALL support categorization of to-do items with custom tags

### Requirement 6: Event Scheduling and Reminders

**User Story:** As a partner, I want to schedule important dates and events, so that neither of us forgets anniversaries or planned activities.

#### Acceptance Criteria

1. WHEN a Partner creates an event with title, date, and time, THE Together System SHALL save the event to the shared calendar
2. WHEN an event date approaches within 24 hours, THE Together System SHALL send reminder notifications to both Partners
3. THE Together System SHALL automatically track and display the relationship start date and calculate days together
4. WHEN a Partner edits or deletes a shared event, THE Together System SHALL synchronize the change to the other Partner's calendar
5. THE Together System SHALL support recurring events with daily, weekly, monthly, or yearly frequency

### Requirement 7: Love Streak and Engagement Tracking

**User Story:** As a partner, I want to see our interaction streak, so that we stay motivated to connect daily.

#### Acceptance Criteria

1. WHEN both Partners interact with the application on the same calendar day, THE Together System SHALL increment the Love Streak counter
2. IF neither Partner interacts with the application for 24 hours, THEN THE Together System SHALL reset the Love Streak counter to zero
3. THE Together System SHALL display the current Love Streak prominently on the Couple Hub dashboard
4. WHEN a Love Streak reaches milestone numbers (7, 30, 100, 365 days), THE Together System SHALL display a celebration notification
5. THE Together System SHALL count interactions as any of: journal entry, mood log, chat message, or challenge completion

### Requirement 8: Couple Challenges and Activities

**User Story:** As a partner, I want to participate in fun challenges with my partner, so that we can strengthen our bond through shared activities.

#### Acceptance Criteria

1. THE Together System SHALL generate a new daily challenge at midnight local time for each Couple Connection
2. WHEN a Partner views available challenges, THE Together System SHALL display challenge description, points value, and completion status
3. WHEN both Partners complete a challenge, THE Together System SHALL award points and update the couple's total score
4. THE Together System SHALL offer challenge categories including communication, fun activities, appreciation, and learning
5. WHEN a challenge expires after 24 hours without completion, THE Together System SHALL archive it and generate a new challenge

### Requirement 9: Virtual Pet Growth System

**User Story:** As a partner, I want to care for a virtual pet together, so that we have a fun shared responsibility that reflects our relationship health.

#### Acceptance Criteria

1. WHEN a Couple Connection is established, THE Together System SHALL create a Virtual Pet at level 1 with basic appearance
2. WHEN Partners complete interactions (messages, challenges, mood logs), THE Together System SHALL increase the Virtual Pet's experience points
3. WHEN the Virtual Pet accumulates sufficient experience points, THE Together System SHALL level up the pet and unlock new appearance options
4. IF Partners do not interact for 3 consecutive days, THEN THE Together System SHALL display the Virtual Pet as sad or neglected
5. THE Together System SHALL allow Partners to customize the Virtual Pet's name and appearance from unlocked options

### Requirement 10: Long-Distance Support Features

**User Story:** As a partner in a long-distance relationship, I want to see the distance between us and time until we meet, so that I feel more connected despite the physical separation.

#### Acceptance Criteria

1. WHERE Users enable location sharing, THE Together System SHALL calculate and display the distance between Partners in kilometers or miles
2. WHEN a Partner sets a "next meeting" date, THE Together System SHALL display a countdown timer showing days, hours, and minutes remaining
3. THE Together System SHALL display both Partners' current local times with timezone labels
4. WHEN Partners are in different timezones, THE Together System SHALL show optimal communication windows based on reasonable hours (8 AM - 10 PM)
5. WHERE network connectivity is unavailable, THE Together System SHALL queue messages for delivery when connection is restored

### Requirement 11: Social Network User Registration and Profiles

**User Story:** As a user, I want to create a public profile and customize my information, so that other users can discover and connect with me.

#### Acceptance Criteria

1. WHEN a User completes registration, THE Together System SHALL create a User Wall with default profile information
2. THE Together System SHALL allow Users to upload a profile picture with maximum size of 2MB in JPG or PNG format
3. WHEN a User updates profile information (bio, interests, location), THE Together System SHALL save changes and display them on the User Wall
4. THE Together System SHALL allow Users to set profile visibility to public, friends-only, or private
5. WHEN a User searches for other Users by username or email, THE Together System SHALL return matching results respecting privacy settings

### Requirement 12: Follow System and Social Connections

**User Story:** As a user, I want to follow other users and accept follow requests, so that I can build a social network and see content from people I care about.

#### Acceptance Criteria

1. WHEN a User sends a follow request to another User, THE Together System SHALL create a pending Follow Relationship request
2. WHEN a User receives a follow request, THE Together System SHALL notify the User and display the request for approval or rejection
3. WHEN a User accepts a follow request, THE Together System SHALL establish a one-way Follow Relationship allowing the follower to see public posts
4. THE Together System SHALL display follower count and following count on each User Wall
5. WHEN a User unfollows another User, THE Together System SHALL remove the Follow Relationship and stop displaying that User's posts in the feed

### Requirement 13: Post Creation and Management

**User Story:** As a user, I want to create posts with text and images, so that I can share my thoughts and experiences with my social network.

#### Acceptance Criteria

1. WHEN a User creates a Post with text content up to 500 characters, THE Together System SHALL publish the Post to the User's Wall
2. THE Together System SHALL allow Users to attach up to 4 images per Post with maximum 5MB per image
3. WHEN a User creates a Post, THE Together System SHALL timestamp the Post and display it in chronological order on the User Wall
4. THE Together System SHALL allow Users to edit their own Posts within 15 minutes of creation
5. WHEN a User deletes a Post, THE Together System SHALL remove the Post and all associated likes and comments

### Requirement 14: Social Feed and Content Discovery

**User Story:** As a user, I want to see posts from users I follow in a personalized feed, so that I can stay updated on their activities and thoughts.

#### Acceptance Criteria

1. WHEN a User opens the social feed, THE Together System SHALL display Posts from all followed Users in reverse chronological order
2. THE Together System SHALL load the 20 most recent Posts initially and support infinite scrolling for older content
3. WHEN a followed User creates a new Post, THE Together System SHALL add the Post to the follower's feed in real-time
4. THE Together System SHALL display Post author information, timestamp, content, and interaction counts (likes, comments)
5. WHERE a User has no Follow Relationships, THE Together System SHALL display suggested users to follow based on mutual connections

### Requirement 15: Post Interactions (Likes and Comments)

**User Story:** As a user, I want to like and comment on posts, so that I can engage with content from my social network.

#### Acceptance Criteria

1. WHEN a User clicks the like button on a Post, THE Together System SHALL increment the like count and record the User's like
2. WHEN a User clicks the like button again on a previously liked Post, THE Together System SHALL remove the like and decrement the count
3. WHEN a User submits a comment on a Post, THE Together System SHALL add the comment with timestamp and display it below the Post
4. THE Together System SHALL display comments in chronological order with author name and profile picture
5. WHEN a Post author receives a like or comment, THE Together System SHALL send a notification to the author

### Requirement 16: Real-time Synchronization

**User Story:** As a user, I want changes made by my partner or followed users to appear immediately, so that the application feels responsive and connected.

#### Acceptance Criteria

1. WHEN a Partner creates a journal entry, THE Together System SHALL push the update to the other Partner's client within 2 seconds
2. WHEN a User creates a Post, THE Together System SHALL deliver the Post to all followers' feeds within 2 seconds
3. WHEN a Partner updates their mood, THE Together System SHALL synchronize the mood change to the other Partner's dashboard within 2 seconds
4. THE Together System SHALL maintain a persistent connection to the Real-time Service while the application is running
5. IF the Real-time Service connection is lost, THEN THE Together System SHALL attempt reconnection every 5 seconds and queue updates locally

### Requirement 17: Couple Hub Dashboard

**User Story:** As a partner, I want to see a unified dashboard showing our relationship status, activities, and important information, so that I can quickly understand our connection health.

#### Acceptance Criteria

1. WHEN a Partner opens the Couple Hub, THE Together System SHALL display the current day summary including partner mood, Love Streak, and upcoming events
2. THE Together System SHALL display the Virtual Pet status and current level prominently on the dashboard
3. THE Together System SHALL show the most recent Together Moments (last 5 activities) from both Partners
4. WHEN a Partner's mood indicates negative emotions, THE Together System SHALL display a supportive message suggestion on the dashboard
5. THE Together System SHALL generate and display a random daily positive activity or conversation starter

### Requirement 18: Data Privacy and Security

**User Story:** As a user, I want my personal data and private conversations to be secure, so that I can trust the application with sensitive information.

#### Acceptance Criteria

1. THE Together System SHALL encrypt all passwords using bcrypt with minimum 10 salt rounds before storage
2. THE Together System SHALL transmit all data between client and server using TLS 1.2 or higher encryption
3. THE Together System SHALL ensure Shared Journal entries are only accessible to the two Partners in the Couple Connection
4. THE Together System SHALL not display private couple data (journal, mood, challenges) to any User outside the Couple Connection
5. WHEN a User deletes their account, THE Together System SHALL permanently remove all personal data within 30 days

### Requirement 19: Offline Capability and Data Synchronization

**User Story:** As a user, I want to use basic features when offline, so that temporary network issues don't prevent me from using the application.

#### Acceptance Criteria

1. WHERE network connectivity is unavailable, THE Together System SHALL allow Users to create journal entries, mood logs, and to-do items locally
2. WHEN network connectivity is restored, THE Together System SHALL synchronize all locally created data to the server within 10 seconds
3. THE Together System SHALL cache the most recent 100 Posts and 30 days of couple data for offline viewing
4. WHERE network connectivity is unavailable, THE Together System SHALL display a clear offline indicator in the user interface
5. THE Together System SHALL prevent actions requiring real-time validation (follow requests, post creation) while offline and display appropriate messages

### Requirement 20: Application Performance and Responsiveness

**User Story:** As a user, I want the application to respond quickly to my actions, so that I have a smooth and enjoyable experience.

#### Acceptance Criteria

1. WHEN a User navigates between application modules, THE Together System SHALL complete the navigation within 500 milliseconds
2. WHEN a User submits a form (post, journal entry, mood log), THE Together System SHALL provide visual feedback within 100 milliseconds
3. THE Together System SHALL load the initial dashboard view within 2 seconds of successful login
4. WHEN displaying images in posts or journals, THE Together System SHALL load thumbnails within 1 second and full resolution within 3 seconds
5. THE Together System SHALL maintain UI responsiveness during background synchronization operations without freezing or stuttering
