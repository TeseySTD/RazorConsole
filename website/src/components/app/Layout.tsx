import { Link, Outlet, useLocation } from "react-router-dom"
import { Github, X, Menu } from "lucide-react"
import { ThemeToggle } from "@/components/app/ThemeToggle"
import { useState, useEffect } from "react"
import { useGitHubStars } from "@/hooks/useGitHubStars"
import { cn } from "@/lib/utils"
import { Button } from "@/components/ui/Button"

export default function Layout() {
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false)
  const { stars } = useGitHubStars("RazorConsole", "RazorConsole")
  const location = useLocation()

  useEffect(() => {
    setMobileMenuOpen(false)
  }, [location.pathname])

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
      <header className="sticky top-0 z-50 w-full border-b border-violet-500/80 bg-white/95 backdrop-blur supports-[backdrop-filter]:bg-white/60 dark:bg-slate-950/95 dark:supports-[backdrop-filter]:bg-slate-950/60">
        <div className={containerClasses}>
          <div className="flex h-16 items-center justify-between">
            {/* Desktop Navigation */}
            <div className="flex items-center gap-8">
              <Link to="/" className="flex items-center space-x-2">
                <span className="bg-gradient-to-r from-blue-500/80 to-violet-500/80 bg-clip-text text-xl font-bold text-transparent dark:from-blue-600 dark:to-violet-600">
                  RazorConsole
                </span>
              </Link>
              <nav className="hidden gap-6 md:flex">
                <Link
                  to="/"
                  className="text-sm font-medium text-slate-600 transition-colors hover:text-slate-900 dark:text-slate-300 dark:hover:text-slate-50"
                >
                  Home
                </Link>
                <Link
                  to="/docs/quick-start"
                  className="text-sm font-medium text-slate-600 transition-colors hover:text-slate-900 dark:text-slate-300 dark:hover:text-slate-50"
                >
                  Docs
                </Link>
                <Link
                  to="/api"
                  className="text-sm font-medium text-slate-600 transition-colors hover:text-slate-900 dark:text-slate-300 dark:hover:text-slate-50"
                >
                  API Reference
                </Link>
                <Link
                  to="/components"
                  className="text-sm font-medium text-slate-600 transition-colors hover:text-slate-900 dark:text-slate-300 dark:hover:text-slate-50"
                >
                  Components
                </Link>
                <Link
                  to="/collaborators"
                  className="text-sm font-medium text-slate-600 transition-colors hover:text-slate-900 dark:text-slate-300 dark:hover:text-slate-50"
                >
                  Collaborators
                </Link>
                <Link
                  to="/showcase"
                  className="text-sm font-medium text-slate-600 transition-colors hover:text-slate-900 dark:text-slate-300 dark:hover:text-slate-50"
                >
                  Showcase
                </Link>
              </nav>
            </div>

            <div className="flex items-center gap-4">
              <ThemeToggle />
              <a
                href="https://github.com/RazorConsole/RazorConsole"
                target="_blank"
                rel="noopener noreferrer"
                className="hidden md:block"
              >
                <Button
                  variant="ghost"
                  size="sm"
                  className="gap-1.5 hover:cursor-pointer"
                  aria-label={stars !== null ? `GitHub - ${stars} stars` : "View on GitHub"}
                >
                  <Github className="h-5 w-5" />
                  {stars !== null && (
                    <span className="text-sm font-medium">
                      {stars >= 1000
                        ? `${(stars / 1000).toFixed(1).replace(/\.0$/, "")}k`
                        : stars.toLocaleString()}
                    </span>
                  )}
                </Button>
              </a>
              {/* Mobile menu button */}
              <Button
                variant="ghost"
                size="icon"
                className="h-10 w-10 rounded-full hover:bg-slate-100 md:hidden dark:hover:bg-slate-800"
                onClick={() => setMobileMenuOpen(true)}
              >
                <Menu className="h-6 w-6" />
              </Button>
            </div>
          </div>
        </div>
      </header>

      {/* Mobile Drawer with Animation */}
      <div
        className={cn(
          "fixed inset-0 z-[60] transition-all duration-300 ease-in-out md:hidden",
          mobileMenuOpen ? "visible" : "pointer-events-none invisible"
        )}
      >
        {/* Backdrop */}
        <div
          className={cn(
            "absolute inset-0 bg-black/80 backdrop-blur-sm transition-opacity duration-300",
            mobileMenuOpen ? "opacity-100" : "opacity-0"
          )}
          onClick={() => setMobileMenuOpen(false)}
        />

        {/* Drawer Panel */}
        <div
          className={cn(
            "absolute inset-y-0 left-0 flex w-3/4 max-w-xs flex-col gap-8 border-r border-slate-200 bg-white p-6 shadow-2xl transition-transform duration-300 ease-in-out dark:border-slate-800 dark:bg-slate-950",
            "overflow-y-auto [&::-webkit-scrollbar]:w-2 [&::-webkit-scrollbar-thumb]:rounded-full [&::-webkit-scrollbar-thumb]:bg-slate-200 hover:[&::-webkit-scrollbar-thumb]:bg-slate-300 dark:[&::-webkit-scrollbar-thumb]:bg-slate-800 dark:hover:[&::-webkit-scrollbar-thumb]:bg-slate-700 [&::-webkit-scrollbar-track]:bg-transparent",
            mobileMenuOpen ? "translate-x-0" : "-translate-x-full"
          )}
        >
          <div className="flex items-center justify-between">
            <span className="bg-gradient-to-r from-blue-500/80 to-violet-500/80 bg-clip-text text-xl font-bold text-transparent dark:from-blue-600 dark:to-violet-600">
              RazorConsole
            </span>
            <Button
              variant="ghost"
              size="icon"
              className="rounded-full"
              onClick={() => setMobileMenuOpen(false)}
            >
              <X className="h-5 w-5" />
            </Button>
          </div>

          {/* Mobile Navigation */}
          <nav className="flex flex-col gap-2">
            <Link
              to="/"
              className="block rounded-md px-4 py-2.5 text-sm font-medium text-slate-700 transition-colors hover:bg-slate-100 dark:text-slate-200 dark:hover:bg-slate-800"
            >
              Home
            </Link>
            <Link
              to="/docs/quick-start"
              className="block rounded-md px-4 py-2.5 text-sm font-medium text-slate-700 transition-colors hover:bg-slate-100 dark:text-slate-200 dark:hover:bg-slate-800"
            >
              Docs
            </Link>
            <Link
              to="/api"
              className="block rounded-md px-4 py-2.5 text-sm font-medium text-slate-700 transition-colors hover:bg-slate-100 dark:text-slate-200 dark:hover:bg-slate-800"
            >
              API Reference
            </Link>
            <Link
              to="/components"
              className="block rounded-md px-4 py-2.5 text-sm font-medium text-slate-700 transition-colors hover:bg-slate-100 dark:text-slate-200 dark:hover:bg-slate-800"
            >
              Components
            </Link>
            <Link
              to="/collaborators"
              className="block rounded-md px-4 py-2.5 text-sm font-medium text-slate-700 transition-colors hover:bg-slate-100 dark:text-slate-200 dark:hover:bg-slate-800"
            >
              Collaborators
            </Link>
            <Link
              to="/showcase"
              className="block rounded-md px-4 py-2.5 text-sm font-medium text-slate-700 transition-colors hover:bg-slate-100 dark:text-slate-200 dark:hover:bg-slate-800"
            >
              Showcase
            </Link>
          </nav>

          <div className="mt-auto border-t border-slate-100 pt-6 dark:border-slate-800">
            <a
              href="https://github.com/RazorConsole/RazorConsole"
              target="_blank"
              rel="noopener noreferrer"
              className="flex items-center gap-3 px-4 py-2 text-sm font-medium text-slate-600 hover:text-slate-900 dark:text-slate-400 dark:hover:text-slate-50"
            >
              <Github className="h-5 w-5" />
              GitHub
            </a>
          </div>
        </div>
      </div>

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
