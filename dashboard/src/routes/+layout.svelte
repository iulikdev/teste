<script lang="ts">
	import '../app.css';
	import { onMount } from 'svelte';
	import { initializeTeams, isInTeams as checkIsInTeams } from '$lib/teams';
	import { isInTeams } from '$lib/stores';

	onMount(async () => {
		if (checkIsInTeams()) {
			const initialized = await initializeTeams();
			isInTeams.set(initialized);
		}
	});
</script>

<slot />
