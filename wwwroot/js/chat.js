// SignalR Chat Client
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chathub", {
        skipNegotiation: false,
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling
    })
    .withAutomaticReconnect([0, 2000, 10000, 30000])
    .configureLogging(signalR.LogLevel.Information)
    .build();

const messagesList = document.getElementById("messagesList");
const messageInput = document.getElementById("messageInput");
const sendButton = document.getElementById("sendButton");
const statusText = document.getElementById("statusText");
const typingIndicator = document.getElementById("typingIndicator");

// Get chat type and selected user ID from window (set by view)
let chatType = window.chatType || "global";
let selectedUserId = window.selectedUserId || null;
const currentUserId = window.currentUserId || null;

// Connection status handlers
connection.onreconnecting(() => {
    statusText.textContent = "Reconnecting...";
    statusText.className = "badge bg-warning";
    sendButton.disabled = true;
});

connection.onreconnected(() => {
    statusText.textContent = "Connected";
    statusText.className = "badge bg-success";
    sendButton.disabled = false;
});

connection.onclose(() => {
    statusText.textContent = "Disconnected";
    statusText.className = "badge bg-danger";
    sendButton.disabled = true;
});

// Message handlers
connection.on("ReceiveMessage", (message) => {
    // Only show global messages in global chat - double check it's not private
    if (chatType === "global" && !message.isPrivate && !message.receiverId) {
        addMessageToUI(message, false);
        scrollToBottom();
    }
});

connection.on("ReceivePrivateMessage", (message) => {
    // Only show private messages if we're in the correct private chat
    // Never show private messages in global chat
    if (chatType === "private" && selectedUserId && 
        (message.userId === selectedUserId || message.receiverId === selectedUserId)) {
        addMessageToUI(message, true);
        scrollToBottom();
    }
    // Explicitly ignore private messages in global chat
});

connection.on("ReceiveError", (error) => {
    alert("Error: " + error);
});

connection.on("UserJoined", (userName) => {
    if (chatType === "global") {
        showSystemMessage(`${userName} joined the chat`);
    }
});

connection.on("UserLeft", (userName) => {
    if (chatType === "global") {
        showSystemMessage(`${userName} left the chat`);
    }
});

// Send message function
sendButton.addEventListener("click", async () => {
    await sendMessage();
});

messageInput.addEventListener("keypress", async (e) => {
    if (e.key === "Enter" && !e.shiftKey) {
        e.preventDefault();
        await sendMessage();
    }
});

async function sendMessage() {
    const message = messageInput.value.trim();
    if (!message) {
        return;
    }

    if (connection.state !== signalR.HubConnectionState.Connected) {
        alert("Not connected to chat. Please wait...");
        return;
    }

    try {
        messageInput.value = "";
        sendButton.disabled = true;

        if (chatType === "private" && selectedUserId) {
            // Send private message
            await connection.invoke("SendPrivateMessage", selectedUserId, message);
        } else {
            // Send global message
            await connection.invoke("SendMessage", message);
        }

        sendButton.disabled = false;
        messageInput.focus();
    } catch (err) {
        console.error("Error sending message:", err);
        alert("Failed to send message. Please try again.");
        sendButton.disabled = false;
        messageInput.value = message;
    }
}

function addMessageToUI(message, isPrivate) {
    // Double-check: Never add private messages to global chat
    if (chatType === "global" && (isPrivate || message.isPrivate || message.receiverId)) {
        console.warn("Attempted to add private message to global chat - ignoring", message);
        return;
    }
    
    // Double-check: Never add global messages to private chat
    if (chatType === "private" && !isPrivate && !message.isPrivate && !message.receiverId) {
        console.warn("Attempted to add global message to private chat - ignoring", message);
        return;
    }
    
    // Check if message already exists (to avoid duplicates)
    const existingMessage = document.querySelector(`[data-message-id="${message.id}"]`);
    if (existingMessage) {
        return;
    }

    const messageDiv = document.createElement("div");
    const isSent = currentUserId && message.userId === currentUserId;
    
    messageDiv.className = `message-item ${isSent ? "message-sent" : "message-received"}`;
    messageDiv.setAttribute("data-message-id", message.id);

    const date = new Date(message.createdAt);
    const formattedDate = formatDate(date);

    messageDiv.innerHTML = `
        <div class="message-header">
            <strong class="message-user">${escapeHtml(message.userName)}</strong>
            <span class="message-time">${formattedDate}</span>
        </div>
        <div class="message-content">${escapeHtml(message.content)}</div>
    `;

    messagesList.appendChild(messageDiv);
}

function showSystemMessage(message) {
    const systemDiv = document.createElement("div");
    systemDiv.className = "message-item";
    systemDiv.style.backgroundColor = "#e7f3ff";
    systemDiv.style.fontStyle = "italic";
    systemDiv.style.textAlign = "center";
    systemDiv.style.color = "#0d6efd";
    systemDiv.style.maxWidth = "100%";
    systemDiv.textContent = message;
    messagesList.appendChild(systemDiv);
    scrollToBottom();
}

function scrollToBottom() {
    const chatContainer = document.getElementById("chatContainer");
    chatContainer.scrollTop = chatContainer.scrollHeight;
}

function formatDate(date) {
    const now = new Date();
    const diff = now - date;
    const seconds = Math.floor(diff / 1000);
    const minutes = Math.floor(seconds / 60);
    const hours = Math.floor(minutes / 60);
    const days = Math.floor(hours / 24);

    if (seconds < 60) {
        return "Just now";
    } else if (minutes < 60) {
        return `${minutes} minute${minutes > 1 ? 's' : ''} ago`;
    } else if (hours < 24) {
        return `${hours} hour${hours > 1 ? 's' : ''} ago`;
    } else if (days < 7) {
        return `${days} day${days > 1 ? 's' : ''} ago`;
    } else {
        const month = date.getMonth() + 1;
        const day = date.getDate();
        const year = date.getFullYear();
        const hours24 = date.getHours();
        const minutes24 = date.getMinutes();
        return `${month}/${day}/${year} ${hours24.toString().padStart(2, '0')}:${minutes24.toString().padStart(2, '0')}`;
    }
}

function escapeHtml(text) {
    const div = document.createElement("div");
    div.textContent = text;
    return div.innerHTML;
}

// Clear messages when switching between chats
function clearMessages() {
    if (messagesList) {
        messagesList.innerHTML = '';
    }
}

// Update chat type when page loads (in case of navigation)
function updateChatType() {
    const newChatType = window.chatType || "global";
    const newSelectedUserId = window.selectedUserId || null;
    
    // Update variables
    chatType = newChatType;
    selectedUserId = newSelectedUserId;
    
    // Don't clear messages on initial page load - server already rendered correct messages
    // Only clear if we're switching chats via JavaScript (not page reload)
}

// Initialize chat type when page loads
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', updateChatType);
} else {
    updateChatType();
}

// Connection state change handler
connection.onclose((error) => {
    if (error) {
        console.error("Connection closed with error:", error);
        statusText.textContent = "Connection lost";
        statusText.className = "badge bg-danger";
    } else {
        console.log("Connection closed");
        statusText.textContent = "Disconnected";
        statusText.className = "badge bg-secondary";
    }
    sendButton.disabled = true;
});

// Start connection with retry logic
async function startConnection() {
    try {
        await connection.start();
        console.log("SignalR Connected");
        statusText.textContent = "Connected";
        statusText.className = "badge bg-success";
        sendButton.disabled = false;
        messageInput.focus();
        scrollToBottom();
    } catch (err) {
        console.error("Error starting connection:", err);
        statusText.textContent = "Connection failed - Retrying...";
        statusText.className = "badge bg-warning";
        sendButton.disabled = true;
        
        // Retry after 5 seconds
        setTimeout(() => {
            if (connection.state === signalR.HubConnectionState.Disconnected) {
                startConnection();
            }
        }, 5000);
    }
}

// Start the connection
startConnection();
