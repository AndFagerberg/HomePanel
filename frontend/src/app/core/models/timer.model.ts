export interface TimerInfo {
  id: string;
  name: string;
  startedAt: string;
  duration: string;
  endsAt: string;
}

export interface CreateTimerRequest {
  name: string;
  duration: string;
}
