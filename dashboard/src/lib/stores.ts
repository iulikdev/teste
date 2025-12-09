import { writable } from 'svelte/store';
import type { CallInfo, CallStatistics } from './types';

export const calls = writable<CallInfo[]>([]);
export const statistics = writable<CallStatistics | null>(null);
export const connectionState = writable<string>('Disconnected');
export const isInTeams = writable<boolean>(false);
