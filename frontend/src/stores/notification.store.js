import { defineStore } from 'pinia';
import * as signalR from '@microsoft/signalr';
import axios from 'axios';
import Swal from 'sweetalert2';

export const useNotificationStore = defineStore('notification', {
  state: () => ({
    notifications: [],
    unreadCount: 0,
    hubConnection: null,
    isConnected: false,
    apiUrl: 'https://localhost:7284/api/notifications',
    hubUrl: 'https://localhost:7284/hubs/notification'
  }),

  actions: {
    async fetchNotifications() {
      try {
        const response = await axios.get(this.apiUrl, {
          withCredentials: true
        });
        this.notifications = response.data;
        this.updateUnreadCount();
      } catch (error) {
        console.error('Error fetching notifications:', error);
      }
    },

    async fetchUnreadCount() {
      try {
        const response = await axios.get(`${this.apiUrl}/unread-count`, {
          withCredentials: true
        });
        this.unreadCount = response.data.unreadCount;
      } catch (error) {
        console.error('Error fetching unread count:', error);
      }
    },

    async markAsRead(id) {
      try {
        await axios.put(`${this.apiUrl}/${id}/read`, {}, {
          withCredentials: true
        });
        
        const index = this.notifications.findIndex(n => n.id === id);
        if (index !== -1 && !this.notifications[index].isRead) {
          this.notifications[index].isRead = true;
          this.unreadCount = Math.max(0, this.unreadCount - 1);
        }
      } catch (error) {
        console.error('Error marking as read:', error);
      }
    },

    async markAllAsRead() {
      try {
        await axios.put(`${this.apiUrl}/read-all`, {}, {
          withCredentials: true
        });
        
        this.notifications.forEach(n => {
          n.isRead = true;
        });
        this.unreadCount = 0;
      } catch (error) {
        console.error('Error marking all as read:', error);
      }
    },

    updateUnreadCount() {
      this.unreadCount = this.notifications.filter(n => !n.isRead).length;
    },

    startHubConnection() {
      if (this.hubConnection && this.isConnected) return;

      this.hubConnection = new signalR.HubConnectionBuilder()
        .withUrl(this.hubUrl, {
          withCredentials: true
        })
        .withAutomaticReconnect()
        .build();

      this.hubConnection.on('ReceiveNotification', (message) => {
        Swal.fire({
            toast: true,
            position: 'bottom-end',
            icon: 'info',
            title: message,
            showConfirmButton: false,
            timer: 5000,
            timerProgressBar: true,
            didOpen: (toast) => {
                toast.onmouseenter = Swal.stopTimer;
                toast.onmouseleave = Swal.resumeTimer;
            }
        });

        // Tự động fetch lại danh sách khi có thông báo mới
        this.fetchNotifications();
      });

      this.hubConnection.start()
        .then(() => {
          this.isConnected = true;
          console.log('SignalR Connected.');
        })
        .catch(err => {
          console.error('SignalR Connection Error: ', err);
          this.isConnected = false;
        });

      this.hubConnection.onclose(() => {
        this.isConnected = false;
        console.log('SignalR Disconnected.');
      });
    },

    stopHubConnection() {
      if (this.hubConnection) {
        this.hubConnection.stop()
          .then(() => {
            this.isConnected = false;
            console.log('SignalR Stopped.');
          });
      }
    }
  }
});
