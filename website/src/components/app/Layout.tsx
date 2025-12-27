import { Link, Outlet, useLocation } from "react-router-dom"

import { cn } from "@/lib/utils"
import { Header } from "@/components/app/Header"

export default function Layout() {
  const location = useLocation()
  const isDocs = location.pathname.startsWith("/docs")
  const isApi = location.pathname.startsWith("/api")
  const isComponents = location.pathname.startsWith("/components")

  const layoutClasses = cn(
    "min-h-screen flex flex-col transition-[padding] duration-300 ease-in-out",
    (isDocs || isApi) && "lg:pl-72",
    isComponents && "md:pl-60 lg:pl-64"
  )

  const containerClasses =
    isDocs || isApi || isComponents ? "w-full px-6 max-w-7xl mx-auto" : "container mx-auto px-4"

  return (
    <div className={layoutClasses}>
      {/* Header */}
      <Header/>

      {/* Main Content */}
      <main className="flex-1">
        <Outlet />
      </main>

      <footer className="border-t bg-slate-50 py-8 dark:bg-slate-900">
        <div className={containerClasses}>
          <div className="grid grid-cols-1 gap-8 md:grid-cols-3">
            <div>
              <h3 className="mb-4 font-semibold">RazorConsole</h3>
              <p className="text-sm text-slate-600 dark:text-slate-400">
                Build rich, interactive console applications using Razor and Spectre.Console
              </p>
            </div>
            <div>
              <h3 className="mb-4 font-semibold">Resources</h3>
              <ul className="space-y-2 text-sm">
                <li>
                  <a
                    href="https://github.com/RazorConsole/RazorConsole"
                    target="_blank"
                    rel="noopener noreferrer"
                    className="text-slate-600 hover:text-slate-900 dark:text-slate-400 dark:hover:text-slate-50"
                  >
                    GitHub Repository
                  </a>
                </li>
                <li>
                  <a
                    href="https://www.nuget.org/packages/RazorConsole.Core"
                    target="_blank"
                    rel="noopener noreferrer"
                    className="text-slate-600 hover:text-slate-900 dark:text-slate-400 dark:hover:text-slate-50"
                  >
                    NuGet Package
                  </a>
                </li>
                <li>
                  <a
                    href="https://discord.gg/DphHAnJxCM"
                    target="_blank"
                    rel="noopener noreferrer"
                    className="text-slate-600 hover:text-slate-900 dark:text-slate-400 dark:hover:text-slate-50"
                  >
                    Discord Community
                  </a>
                </li>
              </ul>
            </div>
            <div>
              <h3 className="mb-4 font-semibold">Documentation</h3>
              <ul className="space-y-2 text-sm">
                <li>
                  <Link
                    to="/docs"
                    className="text-slate-600 hover:text-slate-900 dark:text-slate-400 dark:hover:text-slate-50"
                  >
                    Docs Overview
                  </Link>
                </li>
                <li>
                  <Link
                    to="/api"
                    className="text-slate-600 hover:text-slate-900 dark:text-slate-400 dark:hover:text-slate-50"
                  >
                    API Reference
                  </Link>
                </li>
                <li>
                  <Link
                    to="/docs/quick-start"
                    className="text-slate-600 hover:text-slate-900 dark:text-slate-400 dark:hover:text-slate-50"
                  >
                    Quick Start Guide
                  </Link>
                </li>
                <li>
                  <Link
                    to="/docs/built-in-components"
                    className="text-slate-600 hover:text-slate-900 dark:text-slate-400 dark:hover:text-slate-50"
                  >
                    Built-in Components
                  </Link>
                </li>
                <li>
                  <Link
                    to="/docs/hot-reload"
                    className="text-slate-600 hover:text-slate-900 dark:text-slate-400 dark:hover:text-slate-50"
                  >
                    Advanced Topics
                  </Link>
                </li>
              </ul>
            </div>
          </div>
          <div className="mt-8 border-t pt-8 text-center text-sm text-slate-600 dark:text-slate-400">
            <p>© 2024 RazorConsole. Licensed under MIT License.</p>
          </div>
        </div>
      </footer>
    </div>
  )
}
