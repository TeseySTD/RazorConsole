import { Link } from "react-router-dom"
import { components } from "@/data/components"

export default function ComponentsOverview() {
  return (
    <div className="space-y-8">
      <div>
        <h1 className="mb-4 text-4xl font-bold">Built-in Components</h1>
        <p className="text-lg text-slate-600 dark:text-slate-300">
          RazorConsole ships with 15+ ready-to-use components that wrap Spectre.Console constructs.
        </p>
      </div>
      <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
        {components.map((component) => (
          <Link
            key={component.name}
            to={`/components/${component.name.toLowerCase()}`}
            className="group relative block cursor-pointer rounded-lg border p-6 transition-colors hover:border-slate-400 dark:hover:border-slate-600"
          >
            <h3 className="mb-2 text-xl font-bold">{component.name}</h3>
            <p className="mb-4 text-sm text-slate-500 dark:text-slate-400">
              {component.description}
            </p>
            <span className="inline-flex items-center rounded-md bg-slate-100 px-2 py-1 text-xs font-medium text-slate-600 ring-1 ring-slate-500/10 ring-inset dark:bg-slate-800 dark:text-slate-400 dark:ring-slate-400/20">
              {component.category}
            </span>
          </Link>
        ))}
      </div>
    </div>
  )
}
