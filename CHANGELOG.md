# Changelog

## [1.6.0](https://github.com/paul-freeman/rimworld-freewill/compare/v1.5.0...v1.6.0) (2023-05-13)


### Features

* add initial childcare logic ([#13](https://github.com/paul-freeman/rimworld-freewill/issues/13)) ([8fa7436](https://github.com/paul-freeman/rimworld-freewill/commit/8fa7436a02fcc90f68cba01618a29e376c5a9128))
* consider mech skill level ([#15](https://github.com/paul-freeman/rimworld-freewill/issues/15)) ([4bbfe50](https://github.com/paul-freeman/rimworld-freewill/commit/4bbfe50d90f1fcec65c60dbf2afcb5315a3ba895))
* consider slave suppression need ([#16](https://github.com/paul-freeman/rimworld-freewill/issues/16)) ([fb23874](https://github.com/paul-freeman/rimworld-freewill/commit/fb238744c82d73b2ab82b3c7cfd6be1ee5f59e6c))
* improve free will tool tip ([#17](https://github.com/paul-freeman/rimworld-freewill/issues/17)) ([94796f1](https://github.com/paul-freeman/rimworld-freewill/commit/94796f1657c7664fc599d041a3f35820c5c3a741))
* increase childcare default ([30fee47](https://github.com/paul-freeman/rimworld-freewill/commit/30fee475736e9803b674d9f8894658c443ee7650))
* show freewill priority on work tab ([ae0d195](https://github.com/paul-freeman/rimworld-freewill/commit/ae0d195c79789668488047b439e49a7206d05c43))
* suppress calculations while sleeping ([#18](https://github.com/paul-freeman/rimworld-freewill/issues/18)) ([f02818d](https://github.com/paul-freeman/rimworld-freewill/commit/f02818d4735401ae5f0affb641bdac119613f216))


### Bug Fixes

* setPriorityAction must return string ([#20](https://github.com/paul-freeman/rimworld-freewill/issues/20)) ([633b938](https://github.com/paul-freeman/rimworld-freewill/commit/633b938acab53e2415c72f3384d0829d9d92e4cf))


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
