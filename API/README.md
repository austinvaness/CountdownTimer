To use, copy this file into your project and create an instance of TimerAPI.Timer. Then use the SendToAll and SendTo methods to display the timer on clients. To remove a timer without letting it finish, you may set the length to zero and send the timer to clients again. Removing a timer like this will only work if the timer is counting down.