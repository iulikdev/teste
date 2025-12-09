<script lang="ts">
	import { onMount } from 'svelte';
	import * as microsoftTeams from '@microsoft/teams-js';

	let selectedView = 'dashboard';

	onMount(async () => {
		try {
			await microsoftTeams.app.initialize();

			// Register save handler
			microsoftTeams.pages.config.registerOnSaveHandler((saveEvent) => {
				microsoftTeams.pages.config.setConfig({
					suggestedDisplayName: 'Call Center Dashboard',
					entityId: selectedView,
					contentUrl: `${window.location.origin}/`,
					websiteUrl: `${window.location.origin}/`
				});
				saveEvent.notifySuccess();
			});

			// Enable save button
			microsoftTeams.pages.config.setValidityState(true);
		} catch (error) {
			console.log('Not in Teams context:', error);
		}
	});
</script>

<div class="config-page">
	<h2>Configurare Tab</h2>
	<p>Selecteaza ce vrei sa afisezi in tab:</p>

	<div class="options">
		<label class="option">
			<input type="radio" name="view" value="dashboard" bind:group={selectedView} />
			<span>Dashboard Complet</span>
			<small>Statistici si lista de apeluri</small>
		</label>

		<label class="option">
			<input type="radio" name="view" value="calls" bind:group={selectedView} />
			<span>Doar Apeluri</span>
			<small>Lista apelurilor active</small>
		</label>

		<label class="option">
			<input type="radio" name="view" value="stats" bind:group={selectedView} />
			<span>Doar Statistici</span>
			<small>Statistici call center</small>
		</label>
	</div>
</div>

<style>
	.config-page {
		padding: 20px;
		font-family: 'Segoe UI', sans-serif;
	}

	h2 {
		margin-bottom: 8px;
	}

	p {
		color: #666;
		margin-bottom: 20px;
	}

	.options {
		display: flex;
		flex-direction: column;
		gap: 12px;
	}

	.option {
		display: flex;
		flex-direction: column;
		padding: 16px;
		border: 1px solid #ddd;
		border-radius: 8px;
		cursor: pointer;
		transition: all 0.2s;
	}

	.option:hover {
		border-color: #6264a7;
	}

	.option input {
		position: absolute;
		opacity: 0;
	}

	.option input:checked + span {
		color: #6264a7;
		font-weight: 600;
	}

	.option span {
		font-size: 1rem;
	}

	.option small {
		color: #888;
		font-size: 0.875rem;
		margin-top: 4px;
	}
</style>
