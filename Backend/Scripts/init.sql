-- Drop existing tables in reverse order of dependency to avoid FK errors
DROP TABLE IF EXISTS message_media CASCADE;
DROP TABLE IF EXISTS messages CASCADE;
DROP TABLE IF EXISTS chat_participants CASCADE;
DROP TABLE IF EXISTS chats CASCADE;
DROP TABLE IF EXISTS notifications CASCADE;
DROP TABLE IF EXISTS post_hashtags CASCADE;
DROP TABLE IF EXISTS hashtags CASCADE;
DROP TABLE IF EXISTS reactions CASCADE;
DROP TABLE IF EXISTS comments CASCADE;
DROP TABLE IF EXISTS media CASCADE;
DROP TABLE IF EXISTS posts CASCADE;
DROP TABLE IF EXISTS follows CASCADE;
DROP TABLE IF EXISTS user_settings CASCADE;
DROP TABLE IF EXISTS users CASCADE;

-- Enable the UUID extension if not already enabled
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Core User Components
CREATE TABLE users
(
    user_id             UUID PRIMARY KEY                  DEFAULT uuid_generate_v4(),
    username            VARCHAR(50)              NOT NULL UNIQUE,
    oauth_sub           TEXT UNIQUE              NOT NULL,
    email               VARCHAR(255) UNIQUE,
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
    user_id               UUID PRIMARY KEY REFERENCES users (user_id) ON DELETE CASCADE,
    notifications_enabled BOOLEAN NOT NULL DEFAULT TRUE,
    private_account       BOOLEAN NOT NULL DEFAULT FALSE,
    theme_preference      VARCHAR(20)      DEFAULT 'system', -- e.g., 'light', 'dark', 'system'
    language_preference   VARCHAR(10)      DEFAULT 'en'      -- e.g., 'en', 'es', 'fr'
);

CREATE TABLE follows
(
    follow_id    UUID PRIMARY KEY                  DEFAULT uuid_generate_v4(),
    follower_id  UUID                     NOT NULL REFERENCES users (user_id) ON DELETE CASCADE,
    following_id UUID                     NOT NULL REFERENCES users (user_id) ON DELETE CASCADE,
    created_at   TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    is_approved  BOOLEAN                  NOT NULL DEFAULT TRUE, -- For follow requests on private accounts
    UNIQUE (follower_id, following_id),                          -- Prevent duplicate follow entries
    CHECK (follower_id <> following_id)                          -- Prevent users from following themselves
);

-- Content Components
CREATE TABLE posts
(
    post_id        UUID PRIMARY KEY                  DEFAULT uuid_generate_v4(),
    user_id        UUID                     NOT NULL REFERENCES users (user_id) ON DELETE CASCADE,
    content        TEXT,                                        -- Can be NULL if the post is only media
    created_at     TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at     TIMESTAMP WITH TIME ZONE,
    privacy_level  SMALLINT                 NOT NULL DEFAULT 0, -- e.g., 0: public, 1: followers, 2: specific friends (future), etc.
    reaction_count INTEGER                  NOT NULL DEFAULT 0, -- Denormalized count, update via triggers or app logic
    comment_count  INTEGER                  NOT NULL DEFAULT 0, -- Denormalized count
    share_count    INTEGER                  NOT NULL DEFAULT 0  -- Denormalized count
);

CREATE TABLE media
(
    media_id         UUID PRIMARY KEY                  DEFAULT uuid_generate_v4(),
    parent_id        UUID,
    media_type    VARCHAR(50),  -- E.g., 'image/jpeg', 'video/mp4', 'image/gif'
    -- Discriminates between comment, post,... media.
    -- Nullable because when the link is created, it is not known yet
    media_owner_type VARCHAR(50),
    media_url        VARCHAR(255),
    media_path    VARCHAR(255),
    thumbnail_url VARCHAR(255), -- Optional URL for video/image thumbnails
    uploaded_at      TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE comments
(
    comment_id        UUID PRIMARY KEY                  DEFAULT uuid_generate_v4(),
    post_id           UUID                     NOT NULL REFERENCES posts (post_id) ON DELETE CASCADE,
    user_id           UUID                     NOT NULL REFERENCES users (user_id) ON DELETE CASCADE,
    parent_comment_id UUID REFERENCES comments (comment_id) ON DELETE CASCADE, -- For nested comments (replies)
    content           TEXT                     NOT NULL,
    created_at        TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at        TIMESTAMP WITH TIME ZONE,
    reaction_count    INTEGER                  NOT NULL DEFAULT 0              -- Denormalized count
);

CREATE TABLE reactions
(
    reaction_id   UUID PRIMARY KEY                  DEFAULT uuid_generate_v4(),
    user_id       UUID                     NOT NULL REFERENCES users (user_id) ON DELETE CASCADE,
    content_id    UUID                     NOT NULL,                                             -- Refers to either post_id or comment_id
    content_type  VARCHAR(10)              NOT NULL CHECK (content_type IN ('post', 'comment')), -- Specifies which table content_id refers to
    reaction_type smallint                 NOT NULL,                                             -- Specifies the type of reaction (like, love, laugh, etc.)
    created_at    TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE (user_id, content_id, content_type)                                                   -- Ensures a user can only react once to a specific piece of content
);

CREATE TABLE hashtags
(
    hashtag_id  UUID PRIMARY KEY                  DEFAULT uuid_generate_v4(),
    name        VARCHAR(100)             NOT NULL UNIQUE,    -- Store normalized (e.g., lowercase)
    usage_count INTEGER                  NOT NULL DEFAULT 0, -- Denormalized count, update via triggers or app logic
    first_used  TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE post_hashtags
(
    -- Using composite PK for efficiency and uniqueness guarantee
    post_id    UUID NOT NULL REFERENCES posts (post_id) ON DELETE CASCADE,
    hashtag_id UUID NOT NULL REFERENCES hashtags (hashtag_id) ON DELETE CASCADE,
    PRIMARY KEY (post_id, hashtag_id)
);

-- Activity Tracking
CREATE TABLE notifications
(
    notification_id   UUID PRIMARY KEY                  DEFAULT uuid_generate_v4(),
    user_id           UUID                     NOT NULL REFERENCES users (user_id) ON DELETE CASCADE, -- The user receiving the notification
    actor_user_id     UUID                     REFERENCES users (user_id) ON DELETE SET NULL,         -- Optional: The user who caused the notification (liked, commented, followed)
    notification_type VARCHAR(50)              NOT NULL,                                              -- e.g., 'new_follower', 'like_post', 'comment_post', 'mention', 'like_comment'
    content           TEXT,                                                                           -- Optional brief text content for the notification
    reference_id      UUID,                                                                           -- ID of the related entity (e.g., post_id, comment_id, follower's user_id)
    reference_type    VARCHAR(20),                                                                    -- Clarifies what reference_id refers to (e.g., 'post', 'comment', 'user')
    is_read           BOOLEAN                  NOT NULL DEFAULT FALSE,
    created_at        TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Chat System
CREATE TABLE chats
(
    chat_id       UUID PRIMARY KEY                  DEFAULT uuid_generate_v4(),
    chat_name     VARCHAR(100),                               -- Primarily for group chats
    chat_type     VARCHAR(20)              NOT NULL CHECK (chat_type IN ('direct', 'group')),
    created_at    TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at    TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    message_count INTEGER                  NOT NULL DEFAULT 0 -- Denormalized count, could be useful for chat list UI
);

CREATE TABLE chat_participants
(
    -- Composite PK ensures a user is only in a chat once
    chat_id              UUID                     NOT NULL REFERENCES chats (chat_id) ON DELETE CASCADE,
    user_id              UUID                     NOT NULL REFERENCES users (user_id) ON DELETE CASCADE,
    joined_at            TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    role                 VARCHAR(20)              NOT NULL DEFAULT 'member' CHECK (role IN ('member', 'admin', 'owner')), -- e.g., 'member', 'admin'
    last_read_message_id UUID,                                                                                            -- Tracks the ID of the last message read by this user in this chat
    PRIMARY KEY (chat_id, user_id)
);

CREATE TABLE messages
(
    message_id UUID PRIMARY KEY                  DEFAULT uuid_generate_v4(),
    chat_id    UUID                     NOT NULL REFERENCES chats (chat_id) ON DELETE CASCADE,
    user_id    UUID                     REFERENCES users (user_id) ON DELETE SET NULL, -- Keep message history even if user is deleted, but show as 'deleted user'
    content    TEXT,                                                                   -- Can be NULL if it's only media
    sent_at    TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    is_deleted BOOLEAN                  NOT NULL DEFAULT FALSE                         -- For soft deletes
);

CREATE TABLE message_media
(
    message_media_id UUID PRIMARY KEY                  DEFAULT uuid_generate_v4(),
    message_id       UUID                     NOT NULL REFERENCES messages (message_id) ON DELETE CASCADE,
    media_type       VARCHAR(50),                       -- e.g., 'image/jpeg', 'application/pdf', 'audio/mpeg'
    media_url        VARCHAR(255)             NOT NULL, -- URL to the media file
    file_name        VARCHAR(255),                      -- Optional: original file name
    file_size_bytes  BIGINT,                            -- Optional: file size
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

-- REACTIONS table
CREATE INDEX idx_reactions_user_id ON reactions (user_id);
CREATE INDEX idx_reactions_content ON reactions (content_type, content_id);
-- For fetching reactions for a post/comment

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

-- Renew `updated_at` column on update
CREATE OR REPLACE FUNCTION update_updated_at_column()
    RETURNS TRIGGER AS
$$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- POSTS
CREATE TRIGGER set_updated_at_posts
    BEFORE UPDATE
    ON posts
    FOR EACH ROW
    WHEN (OLD.* IS DISTINCT FROM NEW.*)
EXECUTE FUNCTION update_updated_at_column();

-- COMMENTS
CREATE TRIGGER set_updated_at_comments
    BEFORE UPDATE
    ON comments
    FOR EACH ROW
    WHEN (OLD.* IS DISTINCT FROM NEW.*)
EXECUTE FUNCTION update_updated_at_column();

-- CHATS
CREATE TRIGGER set_updated_at_chats
    BEFORE UPDATE
    ON chats
    FOR EACH ROW
    WHEN (OLD.* IS DISTINCT FROM NEW.*)
EXECUTE FUNCTION update_updated_at_column();