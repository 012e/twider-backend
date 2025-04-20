-- Drop existing tables in reverse order of dependency to avoid FK errors
DROP TABLE IF EXISTS message_media CASCADE;
DROP TABLE IF EXISTS messages CASCADE;
DROP TABLE IF EXISTS chat_participants CASCADE;
DROP TABLE IF EXISTS chats CASCADE;
DROP TABLE IF EXISTS notifications CASCADE;
DROP TABLE IF EXISTS post_hashtags CASCADE;
DROP TABLE IF EXISTS hashtags CASCADE;
DROP TABLE IF EXISTS likes CASCADE;
DROP TABLE IF EXISTS comments CASCADE;
DROP TABLE IF EXISTS media CASCADE;
DROP TABLE IF EXISTS posts CASCADE;
DROP TABLE IF EXISTS follows CASCADE;
DROP TABLE IF EXISTS user_settings CASCADE;
DROP TABLE IF EXISTS users CASCADE;

-- Core User Components
CREATE TABLE users
(
    user_id             SERIAL PRIMARY KEY,
    username            VARCHAR(50)              NOT NULL UNIQUE,
    oauth_sub           TEXT UNIQUE              NOT NULL,
    email               VARCHAR(255)             UNIQUE,
    profile_picture     VARCHAR(255), -- URL or path to the image
    bio                 TEXT,
    created_at          TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    last_login          TIMESTAMP WITH TIME ZONE,
    is_active           BOOLEAN                  NOT NULL DEFAULT TRUE,
    verification_status VARCHAR(20)              NOT NULL DEFAULT 'unverified' CHECK (verification_status IN ('unverified', 'pending', 'verified'))
);

CREATE TABLE user_settings
(
    -- Using user_id as PK enforces a 1-to-1 relationship with users
    user_id               INTEGER PRIMARY KEY REFERENCES users (user_id) ON DELETE CASCADE,
    notifications_enabled BOOLEAN NOT NULL DEFAULT TRUE,
    private_account       BOOLEAN NOT NULL DEFAULT FALSE,
    theme_preference      VARCHAR(20)      DEFAULT 'system', -- e.g., 'light', 'dark', 'system'
    language_preference   VARCHAR(10)      DEFAULT 'en'      -- e.g., 'en', 'es', 'fr'
    -- No setting_id needed if user_id is the PK
);

CREATE TABLE follows
(
    follow_id    SERIAL PRIMARY KEY,
    follower_id  INTEGER                  NOT NULL REFERENCES users (user_id) ON DELETE CASCADE,
    following_id INTEGER                  NOT NULL REFERENCES users (user_id) ON DELETE CASCADE,
    created_at   TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    is_approved  BOOLEAN                  NOT NULL DEFAULT TRUE, -- For follow requests on private accounts
    UNIQUE (follower_id, following_id),                          -- Prevent duplicate follow entries
    CHECK (follower_id <> following_id)                          -- Prevent users from following themselves
);

-- Content Components
CREATE TABLE posts
(
    post_id       SERIAL PRIMARY KEY,
    user_id       INTEGER                  NOT NULL REFERENCES users (user_id) ON DELETE CASCADE,
    content       TEXT,                                        -- Can be NULL if the post is only media
    created_at    TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at    TIMESTAMP WITH TIME ZONE,
    privacy_level SMALLINT                 NOT NULL DEFAULT 0, -- e.g., 0: public, 1: followers, 2: specific friends (future), etc.
    like_count    INTEGER                  NOT NULL DEFAULT 0, -- Denormalized count, update via triggers or app logic
    comment_count INTEGER                  NOT NULL DEFAULT 0, -- Denormalized count
    share_count   INTEGER                  NOT NULL DEFAULT 0  -- Denormalized count
    -- CHECK constraint on content or associated media ensures post isn't empty could be added if needed
);

CREATE TABLE media
(
    media_id      SERIAL PRIMARY KEY,
    post_id       INTEGER                  NOT NULL REFERENCES posts (post_id) ON DELETE CASCADE,
    media_type    VARCHAR(50)              NOT NULL, -- e.g., 'image/jpeg', 'video/mp4', 'image/gif'
    media_url     VARCHAR(255)             NOT NULL, -- URL to the media file (e.g., S3 link)
    thumbnail_url VARCHAR(255),                      -- Optional URL for video/image thumbnails
    uploaded_at   TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
    -- Could add user_id FK here as well if needed for direct user media library management
);

CREATE TABLE comments
(
    comment_id        SERIAL PRIMARY KEY,
    post_id           INTEGER                  NOT NULL REFERENCES posts (post_id) ON DELETE CASCADE,
    user_id           INTEGER                  NOT NULL REFERENCES users (user_id) ON DELETE CASCADE,
    parent_comment_id INTEGER REFERENCES comments (comment_id) ON DELETE CASCADE, -- For nested comments (replies)
    content           TEXT                     NOT NULL,
    created_at        TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at        TIMESTAMP WITH TIME ZONE,
    like_count        INTEGER                  NOT NULL DEFAULT 0                 -- Denormalized count
);

CREATE TABLE likes
(
    like_id      SERIAL PRIMARY KEY,
    user_id      INTEGER                  NOT NULL REFERENCES users (user_id) ON DELETE CASCADE,
    content_id   INTEGER                  NOT NULL,                                             -- Refers to either post_id or comment_id
    content_type VARCHAR(10)              NOT NULL CHECK (content_type IN ('post', 'comment')), -- Specifies which table content_id refers to
    created_at   TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE (user_id, content_id, content_type)                                                  -- Ensures a user can only like a specific piece of content once
);

CREATE TABLE hashtags
(
    hashtag_id  SERIAL PRIMARY KEY,
    name        VARCHAR(100)             NOT NULL UNIQUE,    -- Store normalized (e.g., lowercase)
    usage_count INTEGER                  NOT NULL DEFAULT 0, -- Denormalized count, update via triggers or app logic
    first_used  TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
    -- created_at might be more intuitive than first_used
);

CREATE TABLE post_hashtags
(
    -- Using composite PK for efficiency and uniqueness guarantee
    post_id    INTEGER NOT NULL REFERENCES posts (post_id) ON DELETE CASCADE,
    hashtag_id INTEGER NOT NULL REFERENCES hashtags (hashtag_id) ON DELETE CASCADE,
    PRIMARY KEY (post_id, hashtag_id)
    -- No separate post_hashtag_id needed
);

-- Activity Tracking
CREATE TABLE notifications
(
    notification_id   SERIAL PRIMARY KEY,
    user_id           INTEGER                  NOT NULL REFERENCES users (user_id) ON DELETE CASCADE, -- The user receiving the notification
    actor_user_id     INTEGER                  REFERENCES users (user_id) ON DELETE SET NULL,         -- Optional: The user who caused the notification (liked, commented, followed)
    notification_type VARCHAR(50)              NOT NULL,                                              -- e.g., 'new_follower', 'like_post', 'comment_post', 'mention', 'like_comment'
    content           TEXT,                                                                           -- Optional brief text content for the notification
    reference_id      INTEGER,                                                                        -- ID of the related entity (e.g., post_id, comment_id, follower's user_id)
    reference_type    VARCHAR(20),                                                                    -- Clarifies what reference_id refers to (e.g., 'post', 'comment', 'user')
    is_read           BOOLEAN                  NOT NULL DEFAULT FALSE,
    created_at        TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Chat System
CREATE TABLE chats
(
    chat_id       SERIAL PRIMARY KEY,
    chat_name     VARCHAR(100),                               -- Primarily for group chats
    chat_type     VARCHAR(20)              NOT NULL CHECK (chat_type IN ('direct', 'group')),
    created_at    TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    -- updated_at could track the timestamp of the last message - maintain via trigger/app logic
    updated_at    TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    message_count INTEGER                  NOT NULL DEFAULT 0 -- Denormalized count, could be useful for chat list UI
    -- creator_user_id INTEGER REFERENCES users(user_id) ON DELETE SET NULL -- Optional: track who created the chat
);

CREATE TABLE chat_participants
(
    -- Composite PK ensures a user is only in a chat once
    chat_id              INTEGER                  NOT NULL REFERENCES chats (chat_id) ON DELETE CASCADE,
    user_id              INTEGER                  NOT NULL REFERENCES users (user_id) ON DELETE CASCADE,
    joined_at            TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    role                 VARCHAR(20)              NOT NULL DEFAULT 'member' CHECK (role IN ('member', 'admin', 'owner')), -- e.g., 'member', 'admin'
    last_read_message_id BIGINT,                                                                                          -- Tracks the ID of the last message read by this user in this chat (use BIGINT if message_id is BIGSERIAL)
    -- last_read timestamp might be an alternative to last_read_message_id
    PRIMARY KEY (chat_id, user_id)
);

CREATE TABLE messages
(
    message_id BIGSERIAL PRIMARY KEY,                                                  -- Use BIGSERIAL for potentially high volume
    chat_id    INTEGER                  NOT NULL REFERENCES chats (chat_id) ON DELETE CASCADE,
    user_id    INTEGER                  REFERENCES users (user_id) ON DELETE SET NULL, -- Keep message history even if user is deleted, but show as 'deleted user'
    content    TEXT,                                                                   -- Can be NULL if it's only media
    sent_at    TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    -- is_read is better tracked per participant in chat_participants (using last_read_message_id or similar)
    is_deleted BOOLEAN                  NOT NULL DEFAULT FALSE                         -- For soft deletes
    -- CHECK constraint on content or associated message_media ensures message isn't empty could be added
);

CREATE TABLE message_media
(
    message_media_id SERIAL PRIMARY KEY,
    message_id       BIGINT                   NOT NULL REFERENCES messages (message_id) ON DELETE CASCADE, -- Match BIGINT type
    media_type       VARCHAR(50)              NOT NULL,                                                    -- e.g., 'image/jpeg', 'application/pdf', 'audio/mpeg'
    media_url        VARCHAR(255)             NOT NULL,                                                    -- URL to the media file
    file_name        VARCHAR(255),                                                                         -- Optional: original file name
    file_size_bytes  BIGINT,                                                                               -- Optional: file size
    uploaded_at      TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Add Indexes for common query patterns (examples)
-- Foreign Keys often get indexes automatically, but explicit indexes can be beneficial

-- USERS table
CREATE INDEX idx_users_username ON users (username); -- For login/lookup
CREATE INDEX idx_users_email ON users (email);
CREATE INDEX idx_users_oauth_sub ON users (oauth_sub);
-- For OAuth login
-- For lookup/recovery

-- POSTS table
CREATE INDEX idx_posts_user_id ON posts (user_id);
CREATE INDEX idx_posts_created_at ON posts (created_at DESC);
-- For feeds

-- COMMENTS table
CREATE INDEX idx_comments_post_id ON comments (post_id);
CREATE INDEX idx_comments_user_id ON comments (user_id);
CREATE INDEX idx_comments_parent_comment_id ON comments (parent_comment_id);

-- LIKES table
CREATE INDEX idx_likes_user_id ON likes (user_id);
CREATE INDEX idx_likes_content ON likes (content_type, content_id);
-- For fetching likes for a post/comment

-- FOLLOWS table
CREATE INDEX idx_follows_follower_id ON follows (follower_id);
CREATE INDEX idx_follows_following_id ON follows (following_id);

-- POST_HASHTAGS table
-- Composite PK serves as index, but index on hashtag_id might be useful for finding posts by tag
CREATE INDEX idx_post_hashtags_hashtag_id ON post_hashtags (hashtag_id);

-- NOTIFICATIONS table
CREATE INDEX idx_notifications_user_id ON notifications (user_id);
CREATE INDEX idx_notifications_created_at ON notifications (created_at DESC);

-- CHAT_PARTICIPANTS table
-- Composite PK serves as index, index on user_id useful for finding user's chats
CREATE INDEX idx_chat_participants_user_id ON chat_participants (user_id);

-- MESSAGES table
CREATE INDEX idx_messages_chat_id_sent_at ON messages (chat_id, sent_at DESC); -- Crucial for fetching chat history
CREATE INDEX idx_messages_user_id ON messages (user_id);
-- For finding messages by a user

-- MESSAGE_MEDIA table
CREATE INDEX idx_message_media_message_id ON message_media (message_id);