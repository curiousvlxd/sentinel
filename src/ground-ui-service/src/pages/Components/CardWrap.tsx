const CardWrap = ({ children }: { children: React.ReactNode }) => {
  return (
    <div className="flex w-full items-center justify-center">
      <div
        className="
          w-full
          h-[80vh]
          rounded-2xl
          bg-[#263244]
          shadow-[0_10px_30px_rgba(0,0,0,0.25)]
          relative
        "
      >
        <div
          className="
            h-full
            overflow-y-auto
            px-6 py-4

            [&::-webkit-scrollbar]:w-2
            [&::-webkit-scrollbar-track]:bg-[#1e2a3a] [&::-webkit-scrollbar-track]:rounded-full [&::-webkit-scrollbar-track]:m-1
            [&::-webkit-scrollbar-thumb]:bg-indigo-500/40 [&::-webkit-scrollbar-thumb]:rounded-full
            [&::-webkit-scrollbar-thumb:hover]:bg-indigo-400/60
          "
        >
          {children}
        </div>
      </div>
    </div>
  );
};

export default CardWrap;
