# Changelog

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
