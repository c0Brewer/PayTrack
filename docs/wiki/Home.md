# Time tracking
You have to keep track of the time you spend on the project and its features or tasks.
In order to transparently do so, create tickets in the provided Gitlab project corresponding to the tasks you perform.

To log time write a comment on this issue with the `/spend` slash command.
For example to log 2 hours and 15 minutes create this comment:

`/spend 2h15m`

You should see a message stating that you have added the given amount to the spent time of the issue.

In the wiki a [[Timetracking]] site is automatically added containing a statistical overview of the spent time per user and week.

If you make a mistake, you can remove booked time with `/spend -2h` for example.
NEVER use `/remove_time_spent`!
It will delete every users time.
While GitLab supports an optional date argument to `/spend`, we do not accept that.
You have to book all time in a timely manner.
Therefore all time will be counted at the day you enter it in GitLab.

# Renovate

Outdated components are one major source of vulnerabilities in modern applications.
OWASP Top 10 2021 include this topic as risk number 6 - https://owasp.org/Top10/A06_2021-Vulnerable_and_Outdated_Components/

In order to help you keep your software up to date we have integrated Renovate bot. This bot will create merge requests with updates to your used libraries.
If you want to use it in your project, please create an issue asking for the activation of renovate-bot and assign it to @heimo.stranner.

# GitLab CI

You can use GitLab-CI to test your project. You can find more information and examples to get started on [CI](https://reset.inso-world.com/repo/pub/instructions/-/wikis/CI).


# Kubernetes integration

Continuous Delivery (ADVANCED variant) using Kubernetes is configured for your project.
You can find more information at [k8s](https://reset.inso-world.com/repo/pub/instructions/-/wikis/k8s-ADVANCED).

Your k8s namespace is 26ss-ase-pr-qse-08