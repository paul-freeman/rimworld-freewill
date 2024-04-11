# Changelog

## [1.6.1](https://github.com/paul-freeman/rimworld-freewill/compare/v1.6.0...v1.6.1) (2024-04-11)


### Bug Fixes

* drop support for RimWorld 1.4 ([585f4fd](https://github.com/paul-freeman/rimworld-freewill/commit/585f4fdcfd5c6be9caeb62e488f3149c2ce43c73))

## [1.6.0](https://github.com/paul-freeman/rimworld-freewill/compare/v1.5.0...v1.6.0) (2024-04-09)


### Features

* add 1.5 tag to support RimWorld Anomaly ([2111a8e](https://github.com/paul-freeman/rimworld-freewill/commit/2111a8ed42c73d44070a197356dcfde96f8be31f))
* add flicker to work tab ([853bee9](https://github.com/paul-freeman/rimworld-freewill/commit/853bee92ab860f8fdb2f4c074a16f57e7f569b37))
* add free will ideo preset ([9b63266](https://github.com/paul-freeman/rimworld-freewill/commit/9b632669c47c0731645c8b37913a706d0c3892f1))
* add initial childcare logic ([#13](https://github.com/paul-freeman/rimworld-freewill/issues/13)) ([8fa7436](https://github.com/paul-freeman/rimworld-freewill/commit/8fa7436a02fcc90f68cba01618a29e376c5a9128))
* consider animal pen ([9a531b0](https://github.com/paul-freeman/rimworld-freewill/commit/9a531b0c79265e65ac59de9a01bf311f95591fee))
* consider having food poisoning ([4a7385f](https://github.com/paul-freeman/rimworld-freewill/commit/4a7385f5bfaf7d2da0e4110934f0fb33d4be6991))
* consider mech skill level ([#15](https://github.com/paul-freeman/rimworld-freewill/issues/15)) ([4bbfe50](https://github.com/paul-freeman/rimworld-freewill/commit/4bbfe50d90f1fcec65c60dbf2afcb5315a3ba895))
* consider slave suppression need ([#16](https://github.com/paul-freeman/rimworld-freewill/issues/16)) ([fb23874](https://github.com/paul-freeman/rimworld-freewill/commit/fb238744c82d73b2ab82b3c7cfd6be1ee5f59e6c))
* improve free will tool tip ([#17](https://github.com/paul-freeman/rimworld-freewill/issues/17)) ([94796f1](https://github.com/paul-freeman/rimworld-freewill/commit/94796f1657c7664fc599d041a3f35820c5c3a741))
* increase childcare default ([30fee47](https://github.com/paul-freeman/rimworld-freewill/commit/30fee475736e9803b674d9f8894658c443ee7650))
* increase impact of movement speed ([4d894ea](https://github.com/paul-freeman/rimworld-freewill/commit/4d894ea1e2446ea3c30b5fa612cefc3f41411fd3))
* show freewill priority on work tab ([ae0d195](https://github.com/paul-freeman/rimworld-freewill/commit/ae0d195c79789668488047b439e49a7206d05c43))
* suppress calculations while sleeping ([#18](https://github.com/paul-freeman/rimworld-freewill/issues/18)) ([f02818d](https://github.com/paul-freeman/rimworld-freewill/commit/f02818d4735401ae5f0affb641bdac119613f216))


### Bug Fixes

* check blight works again ([fda5a56](https://github.com/paul-freeman/rimworld-freewill/commit/fda5a56eb0623328045d32d8c751358dd275a764))
* could not get pawns from faction ([bde0b3d](https://github.com/paul-freeman/rimworld-freewill/commit/bde0b3deac1fec634725c34821951813a04b612f))
* error when considering thoughts ([fc2f4b4](https://github.com/paul-freeman/rimworld-freewill/commit/fc2f4b46b46020a0c6d8a2250716006689077f40))
* handle exception getting mood effect ([386201a](https://github.com/paul-freeman/rimworld-freewill/commit/386201af7c65a5252a52cc591be90d601b9d7aa2))
* null when considering other doing ([544e1a6](https://github.com/paul-freeman/rimworld-freewill/commit/544e1a6482c2fbe190964d2851bc7c834fe6032d))
* priorities render while sleeping ([2e559d0](https://github.com/paul-freeman/rimworld-freewill/commit/2e559d0b128c60f5233754063e502737c79ded61))
* set child work correctly ([02ffba2](https://github.com/paul-freeman/rimworld-freewill/commit/02ffba2eb49740f567d854497b2b2ca08862d8bb))
* setPriorityAction must return string ([#20](https://github.com/paul-freeman/rimworld-freewill/issues/20)) ([633b938](https://github.com/paul-freeman/rimworld-freewill/commit/633b938acab53e2415c72f3384d0829d9d92e4cf))
* show game priority in freewill tab ([af1a08a](https://github.com/paul-freeman/rimworld-freewill/commit/af1a08ace6584af289e00642178cfd32ef6d297c))
* update variable to match 1.5 ([169504a](https://github.com/paul-freeman/rimworld-freewill/commit/169504a145405b261f20b6acb219b60ad72dadf3))


### Performance Improvements

* improve mod perfomance ([2599edb](https://github.com/paul-freeman/rimworld-freewill/commit/2599edb4b8d616ea620782ac012b3fe70d32e7fd))

## [1.5.0](https://github.com/paul-freeman/rimworld-freewill/compare/v1.4.0...v1.5.0) (2022-10-24)


### Features

* pawns will consider weapon range before hunting ([#10](https://github.com/paul-freeman/rimworld-freewill/issues/10)) ([16202b9](https://github.com/paul-freeman/rimworld-freewill/commit/16202b99d9835cd757a4694ff8ed48c3d14a3edb))


### Bug Fixes

* boredom memory was not working ([#7](https://github.com/paul-freeman/rimworld-freewill/issues/7)) ([4e4d770](https://github.com/paul-freeman/rimworld-freewill/commit/4e4d7701cd8f4243529fba16fe219a15d14a2035))
* free will does not work during second session ([#9](https://github.com/paul-freeman/rimworld-freewill/issues/9)) ([85c2eff](https://github.com/paul-freeman/rimworld-freewill/commit/85c2eff99670f3efc3746320e138f0cebd02897a))
* pawns now have short memory for boredom ([#4](https://github.com/paul-freeman/rimworld-freewill/issues/4)) ([a406ef7](https://github.com/paul-freeman/rimworld-freewill/commit/a406ef7e96b394468061e977b12bbb6e7ccf89f6))

## v1.4.0

* fixed issue where guest pawns were not able to use free will
* fixed issue where loading a saved game would throw an error
* increase patient and patient bedrest priority when operation is needed
* modified cleaning and hauling logic
* cleaning and hauling tasks will only be prioritized if the local area is not
  beautiful and there is also a cleaning or hauling tasks to do
* some work types will be lowered if there is a cleaning or hauling task that
  needs to be done in the local area
* if error is encountered setting a priority, this will be shown in the free
  will tab
* reconfigured priorities when low food alert is active
* injured prisoners will increase doctoring priority
* roaming animal alert will increase handling priority
* shooting frenzy increases desire to hunt
* deteriorating things in valid storage do not increase desire to haul
* colonists will now consider if they are the best at something and how their
  skills rate relative to others
* the Work Tab mod, if used, must be loaded after this one
* colonists will no longer want to refuel items with autofuel disabled
* added a new mood buff for colonists who desire free will and have not had
  player forced jobs added for some time
* reworked the precept impact level to more accurately show deviation from
  vanilla
* factions without a free will precept will be given the "don't care" free
  will precept at the start of the game
* colonists are less likely to do unnecessary work when others are
  downed/injured
* colonists who are downed, but in a bed, do not affect calculations of downed
  colonists

## v1.0.1

* fixed a bug where colonies with the "don't care" free will precept no longer
  have free will locked

## v1.0.0

Initial release of Free Will. Prior changes can be found in the You Do You
repository.
